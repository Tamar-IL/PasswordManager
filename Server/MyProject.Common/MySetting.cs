namespace MyProject.Common
{
    public class MySetting
    {
        // הגדרות קיימות - הצפנה
        public int numIterationEncrypt { get; set; } = 3;
        public int BlockSize { get; set; } = 78;
        public int subBlockSize { get; set; } = 13;
        public int graphOrder { get; set; } = 13;
        public int keySize { get; set; } = 256;
        public int minPass { get; set; } = 8;
        public int maxPass { get; set; } = 25;
        //public string AESKey { get; set; } = "MyVerySecurePassword123456789012";


        //bbs
        public string P { get; set; } = "18355395351360348063156329298044378798599909632563875456026699993409180455039875665449546958609349409734635539471992816937167116664642902337791173166767320493613041932032334543250892549980816526130181168846925054596071627156093731680866581794026118607728534620597405031905922863519390059837316514576059718776226999152754961294594235488052883010049726637073879693097967080163186619287893497444218252340230715203477178816493539124601299847114971644548253057922284842652291139926429217241899527020646864388376135416558940456815190488195249993780112058364590175374490207551334399548654771113386070362565426632494262827447";
        public string Q { get; set; } = "22116558189093723464220510478382737812082464691029972281352310208547323278950535629797561487726791158142115969564827478292603729693678438606580851930424699072687888913028935266119159877865774412841577325322797419494497351816960135360300914619558329991521299840770133862306473248628975290372909046233697385640938068364465185996083031526531550514462383356525362724390810768375188419189060106014445815641982034692356194110252419119280483818695696130677746317842616816674447782261332427225236374210244414804821890478792561084826294226266135908292999054651042105808871211503940202975505592420134204643283714258989467031059";

        //salt
        public string SaltChars { get; set; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";


        // הגדרות חדשות - JWT טוקנים
        public string JwtSecretKey { get; set; } = "YourVeryLongAndSecureSecretKeyForJWTTokenGenerationThatShouldBeAtLeast256BitsLong!@#$%";
        public string JwtIssuer { get; set; } = "PasswordManagerApp";
        public string JwtAudience { get; set; } = "PasswordManagerUsers";
        public int AccessTokenMinutes { get; set; } = 15;
        //public int RefreshTokenDays { get; set; } = 30;

        // הגדרות אבטחה
        public bool RequireHttps { get; set; } = true;
        public bool SecureCookiesOnly { get; set; } = true;
        public bool HttpOnlyCookies { get; set; } = true;
        public string CookieSameSite { get; set; } = "Strict";

        // הגדרות CSRF
        public bool EnableCsrfProtection { get; set; } = true;
        public string CsrfTokenName { get; set; } = "X-CSRF-TOKEN";
        public int CsrfTokenValidHours { get; set; } = 8;

        // שמות עוגיות קבועים
        public string RefreshTokenCookieName { get; set; } = "__Secure-RefreshToken";
        public string CsrfTokenCookieName { get; set; } = "__Secure-CsrfToken";
    }
}