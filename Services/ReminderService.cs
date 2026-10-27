using Habit_tracker.Models;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace Habit_tracker.Services;

public class ReminderService
{
    const int IdOffset = 10_000;

    public async Task<bool> EnsurePermissionAsync()
    {
        try
        {
            if (await LocalNotificationCenter.Current.AreNotificationsEnabled())
                return true;

            return await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
        catch
        {
            return false;
        }
    }

    public async Task ScheduleAsync(Habit habit)
    {
        try
        {
            if (habit.Id <= 0)
                return;

            Cancel(habit.Id);

            if (!habit.ReminderEnabled)
                return;

            var allowed = await EnsurePermissionAsync();
            if (!allowed)
            {
                habit.ReminderEnabled = false;
                return;
            }

            var timeOfDay = TimeSpan.FromTicks(habit.ReminderTimeTicks);
            if (timeOfDay < TimeSpan.Zero || timeOfDay >= TimeSpan.FromDays(1))
                timeOfDay = TimeSpan.FromHours(9);

            var notifyTime = DateTime.Today.Add(timeOfDay);
            if (notifyTime <= DateTime.Now)
                notifyTime = notifyTime.AddDays(1);

            var subtitle = habit.IsDailyGoal
                ? "Daily streak reminder"
                : $"Weekly goal: {habit.TargetPerWeek}× — keep going";

            var request = new NotificationRequest
            {
                NotificationId = IdOffset + habit.Id,
                Title = $"Reminder: {habit.Title}",
                Subtitle = subtitle,
                Description = string.IsNullOrWhiteSpace(habit.Description)
                    ? "A gentle nudge — show up for yourself today."
                    : habit.Description,
                CategoryType = NotificationCategoryType.Reminder,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime,
                    RepeatType = NotificationRepeat.Daily
                },
                Android = new AndroidOptions
                {
                    ChannelId = "habit_reminders",
                    Priority = AndroidPriority.High
                }
            };

            await LocalNotificationCenter.Current.Show(request);
        }
        catch
        {
            // Never crash the app because of notifications
            habit.ReminderEnabled = false;
        }
    }

    public void Cancel(int habitId)
    {
        try
        {
            if (habitId <= 0) return;
            LocalNotificationCenter.Current.Cancel(IdOffset + habitId);
        }
        catch
        {
            // ignore
        }
    }

    public async Task RescheduleAllAsync(IEnumerable<Habit> habits)
    {
        foreach (var habit in habits)
            await ScheduleAsync(habit);
    }
}
