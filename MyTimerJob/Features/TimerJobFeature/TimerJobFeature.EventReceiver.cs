using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using TJ = MyTimerJob.Timer_Jobs;

namespace MyTimerJob.Features.TimerJobFeature
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("33def1c8-8d45-4c03-be1e-bb7d9922d582")]
    public class TimerJobFeatureEventReceiver : SPFeatureReceiver
    {
        
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                // Delete any existing jobs
                SPWebApplication webApp = properties.Feature.Parent as SPWebApplication;
                DeleteJob(webApp.JobDefinitions);

                // Create the job.
                TJ.MyTimerJob job =
                    new TJ.MyTimerJob(TJ.MyTimerJob.jobName, webApp);

                // Create the schedule so that the job runs hourly, sometime 
                // during the first quarter of the hour.
                SPMinuteSchedule schedule = new SPMinuteSchedule();
                schedule.BeginSecond = 0;
                schedule.EndSecond = 59;
                schedule.Interval = 30;
                job.Schedule = schedule;
                job.Update();

                // Configure the job.
                TJ.MyTimerJobSettings jobSettings = new TJ.MyTimerJobSettings(
                    webApp, Guid.NewGuid());
                jobSettings.WOEID = "";

                jobSettings.Update(true);
            }
            catch (Exception ex)
            {
                TJ.ULSLogger.Instance.LogError(ex);
            }
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            try
            {
                SPWebApplication webApp = properties.Feature.Parent as SPWebApplication;
                DeleteJob(webApp.JobDefinitions);

                // Delete the job's settings.
                TJ.MyTimerJobSettings jobSettings =
                    webApp.GetChild<TJ.MyTimerJobSettings>
                        (TJ.MyTimerJobSettings.SettingsName);
                if (jobSettings != null)
                {
                    jobSettings.Delete();
                }
            }
            catch (Exception ex)
            {
                TJ.ULSLogger.Instance.LogError(ex);
            }
        }

        #region Private Methods
        private void DeleteJob(SPJobDefinitionCollection jobs)
        {
            foreach (SPJobDefinition job in jobs)
            {
                if (job.Name.Equals(TJ.MyTimerJob.jobName,
                            StringComparison.OrdinalIgnoreCase))
                {
                    job.Delete();
                }
            }
        }
        private void DeleteJobAndSettings(SPService service)
        {
            // Find the job and delete it.
            foreach (SPJobDefinition job in service.JobDefinitions)
            {
                if (job.Name == TJ.MyTimerJob.jobName)
                {
                    job.Delete();
                    break;
                }
            }

            // Delete the job's settings.
            TJ.MyTimerJobSettings jobSettings = service.GetChild<
                 TJ.MyTimerJobSettings>(
                     TJ.MyTimerJobSettings.SettingsName);
            if (jobSettings != null)
            {
                jobSettings.Delete();
            }
        }
        #endregion
    }
}
