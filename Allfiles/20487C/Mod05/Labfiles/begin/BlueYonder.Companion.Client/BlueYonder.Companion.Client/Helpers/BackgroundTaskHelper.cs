using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace BlueYonder.Companion.Client.Helpers
{
    public class BackgroundTaskHelper
    {
        public static void RegisterBackgroundTaskForWeather()
        {
            var taskName = Constants.WeatherTaskName;

            IBackgroundTaskRegistration task = FindRegisteredTask(taskName);
            if (task == null)
            {
                var builder = new BackgroundTaskBuilder();
                builder.Name = taskName;
                builder.TaskEntryPoint = Constants.WeatherTaskEntryPoint;

                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

                builder.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));

                builder.SetTrigger(new TimeTrigger(15, false));

                task = builder.Register();

                var settings = ApplicationData.Current.LocalSettings;
                var key = task.TaskId.ToString();
                settings.Values[taskName] = key;
            }
        }

        public static void UnregisterWeatherTask(string taskName)
        {
            var task = FindRegisteredTask(taskName);
            if (task != null)
            {
                task.Unregister(true);
                var settings = ApplicationData.Current.LocalSettings;
                settings.Values[taskName] = string.Empty;
            }
        }

        public static IBackgroundTaskRegistration FindRegisteredTask(string taskName)
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    return task.Value;
                }
            }
            return null;
        }
    }
}
