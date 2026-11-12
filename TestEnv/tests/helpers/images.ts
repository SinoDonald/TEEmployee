import * as fs from 'fs';
import * as path from 'path';
import { FIXTURES } from './env';

export interface FilePayload {
  name: string;
  mimeType: string;
  buffer: Buffer;
}

export function fixture(name: string, mimeType?: string): FilePayload {
  const buffer = fs.readFileSync(path.join(FIXTURES, name));
  const ext = path.extname(name).toLowerCase();
  const guess = ext === '.jpg' || ext === '.jpeg' ? 'image/jpeg' : ext === '.png' ? 'image/png' : ext === '.gif' ? 'image/gif' : ext === '.svg' ? 'image/svg+xml' : 'application/octet-stream';
  return { name, mimeType: mimeType || guess, buffer };
}

/** 產生超過指定大小的「PNG」(檔頭正確、內容是雜訊)，用來測試大小限制 */
export function oversizedPng(bytes: number): FilePayload {
  const header = Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);
  const body = Buffer.alloc(bytes, 7);
  return { name: 'huge.png', mimeType: 'image/png', buffer: Buffer.concat([header, body]) };
}

/** 讀取 JPEG / PNG / GIF 的寬高 */
export function imageSize(buf: Buffer): { width: number; height: number; type: string } | null {
  if (buf.length > 24 && buf.readUInt32BE(0) === 0x89504e47) {
    return { type: 'png', width: buf.readUInt32BE(16), height: buf.readUInt32BE(20) };
  }
  if (buf.length > 10 && buf.toString('ascii', 0, 3) === 'GIF') {
    return { type: 'gif', width: buf.readUInt16LE(6), height: buf.readUInt16LE(8) };
  }
  if (buf.length > 4 && buf[0] === 0xff && buf[1] === 0xd8) {
    let i = 2;
    while (i < buf.length) {
      if (buf[i] !== 0xff) { i++; continue; }
      const marker = buf[i + 1];
      if (marker === 0xd8 || marker === 0x01 || (marker >= 0xd0 && marker <= 0xd7)) { i += 2; continue; }
      const len = buf.readUInt16BE(i + 2);
      // SOF0..SOF15 (排除 DHT C4、JPG C8、DAC CC)
      if (marker >= 0xc0 && marker <= 0xcf && marker !== 0xc4 && marker !== 0xc8 && marker !== 0xcc) {
        return { type: 'jpeg', height: buf.readUInt16BE(i + 5), width: buf.readUInt16BE(i + 7) };
      }
      i += 2 + len;
    }
  }
  return null;
}

/** JPEG 是否含有 EXIF (APP1 "Exif") 區段 */
export function hasExif(buf: Buffer): boolean {
  if (!(buf[0] === 0xff && buf[1] === 0xd8)) return false;
  let i = 2;
  while (i + 4 < buf.length) {
    if (buf[i] !== 0xff) return false;
    const marker = buf[i + 1];
    if (marker === 0xda) return false; // 影像資料開始
    const len = buf.readUInt16BE(i + 2);
    if (marker === 0xe1 && buf.toString('ascii', i + 4, i + 8) === 'Exif') return true;
    i += 2 + len;
  }
  return false;
}

/** 檔案內容是否含有指定文字 (用來檢查 EXIF 中的手機型號是否被移除) */
export function containsText(buf: Buffer, text: string): boolean {
  return buf.includes(Buffer.from(text, 'latin1'));
}
