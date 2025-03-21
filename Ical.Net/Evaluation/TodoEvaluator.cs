﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Evaluation;

public class TodoEvaluator : RecurringEvaluator
{
    protected Todo Todo => Recurrable as Todo ?? throw new InvalidOperationException();

    public TodoEvaluator(Todo todo) : base(todo) { }

    internal IEnumerable<Period> EvaluateToPreviousOccurrence(CalDateTime completedDate, CalDateTime currDt, EvaluationOptions options)
    {
        var beginningDate = completedDate.Copy();

        if (Todo.RecurrenceRules != null)
        {
            foreach (var rrule in Todo.RecurrenceRules)
            {
                DetermineStartingRecurrence(rrule, ref beginningDate);
            }
        }

        DetermineStartingRecurrence(Todo.RecurrenceDates.GetAllPeriods(), ref beginningDate);
        DetermineStartingRecurrence(Todo.RecurrenceDates.GetAllDates(), ref beginningDate);

        if (Todo.ExceptionRules != null)
        {
            foreach (var exrule in Todo.ExceptionRules)
            {
                DetermineStartingRecurrence(exrule, ref beginningDate);
            }
        }

        DetermineStartingRecurrence(Todo.ExceptionDates.GetAllDates(), ref beginningDate);

        return Evaluate(Todo.Start, beginningDate, currDt.AddSeconds(1), options);
    }

    private static void DetermineStartingRecurrence(IEnumerable<Period> rdate, ref CalDateTime referenceDateTime)
    {
        var dt2 = referenceDateTime;
        foreach (var p in rdate.Where(p => p.StartTime.LessThan(dt2)))
        {
            referenceDateTime = p.StartTime;
        }
    }

    private static void DetermineStartingRecurrence(IEnumerable<CalDateTime> rdate, ref CalDateTime referenceDateTime)
    {
        var dt2 = referenceDateTime;
        foreach (var dt in rdate.Where(dt => dt.LessThan(dt2)))
        {
            referenceDateTime = dt;
        }
    }

    private void DetermineStartingRecurrence(RecurrencePattern recur, ref CalDateTime referenceDateTime)
    {
        if (recur.Count.HasValue)
        {
            referenceDateTime = Todo.Start.Copy();
        }
        else
        {
            IncrementDate(ref referenceDateTime, recur, -recur.Interval);
        }
    }

    public override IEnumerable<Period> Evaluate(CalDateTime referenceDate, CalDateTime? periodStart, CalDateTime? periodEnd, EvaluationOptions options)
    {
        // TODO items can only recur if a start date is specified
        if (Todo.Start == null)
            return [];

        return base.Evaluate(referenceDate, periodStart, periodEnd, options)
            .Select(p => p);
    }
}
