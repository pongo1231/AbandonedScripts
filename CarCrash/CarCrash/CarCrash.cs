using CitizenFX.Core;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarCrash
{
    public class CarCrash : BaseScript
    {
        private int prevHealth = -1;

        public CarCrash()
        {
            Tick += new Func<Task>(async delegate
            {
                Vehicle car = LocalPlayer.Character.CurrentVehicle;
                if (car == null)
                {
                    return;
                }

                int currentHealth = Convert.ToInt32(car.EngineHealth);
                if (prevHealth == -1)
                {
                    prevHealth = currentHealth;
                }
                else
                {
                    if (currentHealth <= 500)
                    {
                        car.FuelLevel = 0;
                        Screen.DisplayHelpTextThisFrame("This car has been totalled.");
                    }
                    else if (currentHealth <= prevHealth - 30)
                    {
                        float prevFuel = car.FuelLevel;
                        for (int i = 0; i < 250; i++)
                        {
                            Screen.DisplayHelpTextThisFrame("The car engine has been disabled for 7 seconds.");
                            car.FuelLevel = 0;
                            await Delay(1);
                        }
                        car.FuelLevel = prevFuel;
                    }

                    prevHealth = currentHealth;
                }
            });
        }
    }
}
