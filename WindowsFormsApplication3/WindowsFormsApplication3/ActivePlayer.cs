namespace WindowsFormsApplication3
{
    public class ActivePlayer
    {
        public ChampionStats Stats { get; set; }
        public bool IsDead;
        private ActivePlayer(bool isDead = false)
        {
            IsDead = isDead;
        }

        public static ActivePlayer FromData(dynamic data, bool isDead = false)
        {
            var player = new ActivePlayer(isDead);
            player.UpdateFromData(data);
            return player;
        }
        public void UpdateFromData(dynamic data)
        {
            //this.Abilities = AbilityLoadout.FromData(data.abilities);
            this.Stats = (data.championStats as Newtonsoft.Json.Linq.JObject).ToObject<ChampionStats>();
        }
    }
}