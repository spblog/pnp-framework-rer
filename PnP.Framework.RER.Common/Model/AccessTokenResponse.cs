namespace PnP.Framework.RER.Common.Model
{
    public class AccessTokenResponse
    {
        public string access_token { get; set; }
        public long expires_on { get; set; }
        public long not_before { get; set; }
        public int expires_in { get; set; }
    }
}
