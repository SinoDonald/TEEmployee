﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEEmployee.Models.Profession
{
    interface IProfessionRepository : IDisposable
    {
        List<Skill> GetAll(); // test
        List<Skill> GetAllSkillsByRole(List<string> roles); // For Skills page
        List<Skill> GetAllScoresByRole(List<string> roles); // For Scores page
        List<Skill> UpsertSkills(List<Skill> skills);
        bool UpsertScores(List<Score> scores);
        bool DeleteSkills(List<Skill> skills);
        //Schedule Update(Schedule schedule);
        //bool Update(List<Schedule> schedules);
        //Schedule Insert(Schedule schedule);
        //bool Delete(List<Schedule> schedules);

    }
}