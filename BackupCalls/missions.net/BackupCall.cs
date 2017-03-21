using CitizenFX.Core;
using NativeUI;
using System;
using System.Threading.Tasks;

namespace missions.net
{
    public class BackupCall : BaseScript
    {
        private int cooldown = 0;
        private UIMenuItem backupBuzzardItem;
        private UIMenuItem backupTechnicalItem;

        public BackupCall()
        {
            MenuPool menuPool = new MenuPool();

            UIMenu menu = new UIMenu("Backup Menu", "");
            menuPool.Add(menu);

            AddBackupHeliItem(menu);
            AddBackupTechnicalItem(menu);

            menu.RefreshIndex();

            Tick += new Func<Task>(async delegate
            {
                menuPool.ProcessMenus();
                if (Game.IsControlJustReleased(0, Control.PhoneOption))
                {
                    menu.Visible = !menu.Visible;
                    ClearDescriptions();
                }

                await Task.FromResult(0);
            });
        }

        private void AddBackupHeliItem(UIMenu menu)
        {
            backupBuzzardItem = new UIMenuItem("Request Heli Backup");
            menu.AddItem(backupBuzzardItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == backupBuzzardItem)
                {
                    RequestBackup("bc:buzzardbackup", backupBuzzardItem);
                }
            };
        }

        private void AddBackupTechnicalItem(UIMenu menu)
        {
            backupTechnicalItem = new UIMenuItem("Request Technical Backup");
            menu.AddItem(backupTechnicalItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == backupTechnicalItem)
                {
                    RequestBackup("bc:technicalbackup", backupTechnicalItem);
                }
            };
        }

        private async void RequestBackup(string eventName, UIMenuItem item)
        {
            if (cooldown > 0)
            {
                item.Description = $"You have to wait {cooldown / 60}:{cooldown % 60} minutes before requesting another backup.";
            }
            else
            {
                TriggerEvent(eventName);
                item.Description = "Backup on their way.";

                // 6:30 minutes countdown
                for (cooldown = 390; cooldown > 0; cooldown--)
                {
                    await Delay(1000);
                }
            }
        }

        private void ClearDescriptions()
        {
            backupBuzzardItem.Description = null;
            backupTechnicalItem.Description = null;
        }
    }
}
