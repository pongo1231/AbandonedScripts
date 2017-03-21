using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PedTest {
    class Test : BaseScript {

        public Test() {
            Tick += OnTick;
        }

        private async Task OnTick() {
            Ped playerPed = LocalPlayer.Character;
            if (playerPed == null) {
                return;
            }

            Vector3 playerPos = playerPed.Position;
            ApplyRandomForce(GetClosestVehicle(playerPos, 0));

            await Task.FromResult(0);
        }

        private Vehicle GetClosestVehicle(Vector3 pos, int hash) {
            int vehicleHandle = Function.Call<int>(Hash.GET_CLOSEST_VEHICLE, pos.X, pos.Y, pos.Z, float.MaxValue, hash, 70);
            TriggerEvent("chatMessage", "", new int[] { 0, 0, 0 }, vehicleHandle);
            return vehicleHandle == 0 ? new Vehicle(vehicleHandle) : null;
        }

        private void ApplyRandomForce(Vehicle car) {
            Random random = new Random((int) ConvertToUnixTimestamp(DateTime.Now));
            car.ApplyForce(new Vector3(random.Next(10), random.Next(10), random.Next(10)));
        }

        private static double ConvertToUnixTimestamp(DateTime date) {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
    }
}
