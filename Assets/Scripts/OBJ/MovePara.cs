using UnityEngine;
using System.Collections.Generic;

public class MovePara {
    protected float[,] hip_rot_run;
    protected float[,] leftup_leg_rot_run;
    protected float[,] rightup_leg_rot_run;
    protected float[,] left_leg_rot_run;
    protected float[,] right_leg_rot_run;
    protected float[,] spine_rot_run;
    protected float[,] left_arm_rot_run;
    protected float[,] right_arm_rot_run;
    protected float[,] leftfore_arm_rot_run;
    protected float[,] rightfore_arm_rot_run;
    protected float[,] neck_run;
    protected int step;

	public MovePara()
    {
        step = 0;

        hip_rot_run = new float[,]{
		{-1.615326f, 1.8896f, -2.606262f},
		{-1.676636f, 0.2767f, -2.571808f},
		{-0.177948f, 1.5713f, -2.361115f},
		{-3.026245f, -4.4888f, -1.585693f},
		{-3.822876f, -5.5417f, -1.056061f},
		{-4.274994f, -7.6151f, -0.9126282f},
		{-4.484741f, -8.1624f, -1.148071f},
		{-4.424896f, -9.3506f, -1.989441f},
		{-4.328644f, -10.087f, -2.732147f},
		{-4.300323f, -10.5037f, -3.212372f},
		{-4.200043f, -10.0335f, -3.118652f},
		{-4.000641f, -9.5495f, -2.159637f},
		{-3.660461f, -9.6579f, -0.9437866f},
		{-3.134583f, -9.0741f, 0.4136253f},
		{-2.455627f, -9.3688f, 1.49179f},
		{-1.548798f, -10.5865f, 2.028861f},
		{-0.7808533f, -10.3332f, 2.310915f},
		{-0.2567749f, -10.9908f, 2.456488f},
		{-0.1891479f, -9.0902f, 2.352742f},
		{-0.7881775f, -7.5605f, 1.944898f},
		{-1.737335f, -5.2247f, 1.191549f},
		{-3.082153f, -4.9232f, -0.1734314f},
		{-4.220886f, -2.6929f, -1.373413f},
		{-4.27951f, 0.4491f, -1.927246f},
		{-4.766968f, 1.7234f, -2.132935f},
		{-4.525146f, 3.8632f, -2.079102f},
		{-4.272461f, 5.3528f, -1.797241f},
		{-4.027924f, 6.2007f, -1.274017f},
		{-3.722748f, 6.346f, -1.011017f},
		{-3.348297f, 5.7682f, -1.03299f},
		{-3.019775f, 5.2614f, -1.213318f},
		{-2.741211f, 4.825f, -1.553894f},
		{-2.436035f, 4.5409f, -1.88147f},
		{-2.095062f, 4.4156f, -2.200226f},
		{-1.82724f, 3.8835f, -2.432129f},
		{-1.647858f, 2.9248f, -2.570343f}};


        leftup_leg_rot_run = new float[,]{
		{-29.34863f, 180f, -11.10544f},
		{-27.11087f, 180f, -10.72192f},
		{-22.98221f, 180f, -11.11624f},
		{-24.36224f, 180f, -7.96344f},
		{-21.91428f, 180f, -6.787323f},
		{-17.85373f, 180f, -6.75946f},
		{-11.37137f, 180f, -7.492126f},
		{-2.158264f, 180f, -8.973602f},
		{6.823833f, 180f, -10.29819f},
		{13.60468f, 180f, -11.55426f},
		{20.92694f, 180f, -11.69333f},
		{30.43828f, 180f, -9.877136f},
		{37.26474f, 180f, -8.264191f},
		{39.08548f, 180f, -7.882843f},
		{39.32697f, 180f, -7.712341f},
		{39.11896f, 180f, -7.46048f},
		{37.86272f, 180f, -7.187347f},
		{35.64859f, 180f, -6.639709f},
		{31.95607f, 180f, -6.5849f},
		{26.59153f, 180f, -7.452118f},
		{19.26169f, 180f, -8.846405f},
		{9.083383f, 180f, -10.8129f},
		{-1.473511f, 180f, -12.2709f},
		{-11.29297f, 180f, -12.69028f},
		{-22.86746f, 180f, -13.26254f},
		{-30.37146f, 180f, -14.18387f},
		{-36.57312f, 180f, -14.48587f},
		{-41.38379f, 180f, -14.29462f},
		{-44.64563f, 180f, -13.65897f},
		{-46.35986f, 180f, -12.64667f},
		{-46.56375f, 180f, -11.93634f},
		{-45.2504f, 180f, -11.51877f},
		{-42.58746f, 180f, -11.41617f},
		{-38.4873f, 180f, -11.60248f},
		{-34.80124f, 180f, -11.5914f},
		{-31.58957f, 180f, -11.38278f}};


        rightup_leg_rot_run = new float[,]{
		{23.04431f, 180f, -1.537567f},
		{17.73972f, 180f, -0.7106934f},
		{12.31095f, 180f, 1.650862f},
		{0.6586712f, 180f, 4.012578f},
		{-8.665375f, 180f, 6.745022f},
		{-17.99124f, 180f, 8.842381f},
		{-28.95474f, 180f, 9.76898f},
		{-36.2294f, 180f, 9.450106f},
		{-41.2948f, 180f, 8.642545f},
		{-44.5625f, 180f, 7.383412f},
		{-46.19211f, 180f, 6.643553f},
		{-46.33142f, 180f, 6.845124f},
		{-44.95224f, 180f, 7.606417f},
		{-41.90076f, 180f, 9.047096f},
		{-37.82703f, 180f, 10.32774f},
		{-32.55521f, 180f, 11.05341f},
		{-27.91422f, 180f, 11.21486f},
		{-24.44937f, 180f, 10.7436f},
		{-23.35086f, 180f, 9.899491f},
		{-26.50275f, 180f, 8.829663f},
		{-29.00079f, 180f, 7.396759f},
		{-28.64087f, 180f, 5.399051f},
		{-26.07422f, 180f, 3.564517f},
		{-19.84158f, 180f, 1.953262f},
		{-12.39618f, 180f, 1.232157f},
		{-4.07077f, 180f, 0.7822121f},
		{4.676544f, 180f, 0.8320997f},
		{13.89856f, 180f, 1.310677f},
		{21.79944f, 180f, 1.293684f},
		{28.37234f, 180f, 0.8866456f},
		{32.56664f, 180f, 0.6229046f},
		{34.36988f, 180f, 0.5647661f},
		{34.51356f, 180f, 0.1096022f},
		{33.01798f, 180f, -0.7902222f},
		{30.43114f, 180f, -1.401672f},
		{26.76467f, 180f, -1.730652f}};


        left_leg_rot_run = new float[,]{
		{-6.543915f, 180f, -11.29733f},
		{-8.717438f, 180f, -11.53171f},
		{-12.44247f, 180f, -12.56848f},
		{-19.66736f, 180f, -12.47537f},
		{-25.56909f, 180f, -12.78296f},
		{-30.23505f, 180f, -12.49881f},
		{-31.46301f, 180f, -11.87228f},
		{-27.41583f, 180f, -11.12906f},
		{-23.11041f, 180f, -10.9039f},
		{-20.92532f, 180f, -11.11154f},
		{-16.51373f, 180f, -11.39603f},
		{-6.159821f, 180f, -11.32443f},
		{-1.575134f, 180f, -11.21686f},
		{-8.566376f, 180f, -12.04364f},
		{-18.89276f, 180f, -13.36624f},
		{-30.49228f, 180f, -12.89056f},
		{-42.2514f, 180f, -11.20972f},
		{-53.32626f, 180f, -9.743256f},
		{-63.948f, 180f, -8.55069f},
		{-74.23959f, 180f, -8.088684f},
		{-83.6604f, 180f, -7.331543f},
		{-90f, 174.6655f, 0f},
		{-81.44031f, 7.170241E-07f, 176.8156f},
		{-78.49084f, 5.348804E-07f, 177.5496f},
		{-77.32159f, 1.215629E-07f, 179.4237f},
		{-78.77377f, 0f, 179.2972f},
		{-82.82697f, 4.273451E-07f, 178.6236f},
		{-90f, 177.3101f, 0f},
		{-82.1467f, 180f, -3.958344f},
		{-72.05594f, 180f, -5.147186f},
		{-60.63623f, 180f, -6.425476f},
		{-47.77649f, 180f, -7.728882f},
		{-34.94522f, 180f, -9.029663f},
		{-21.79486f, 180f, -10.18738f},
		{-12.67899f, 180f, -10.86084f},
		{-7.789032f, 180f, -11.17523f}};


        right_leg_rot_run = new float[,]{
		{-71.52679f, 180f, 10.535f},
		{-82.48203f, 180f, 12.0272f},
		{-90f, 190.3515f, 0f},
		{-79.147f, 0f, 194.2608f},
		{-73.20386f, -1.477282E-06f, 194.5323f},
		{-70.36017f, 0f, 194.698f},
		{-70.88852f, 0f, 194.0595f},
		{-75.03198f, -1.652809E-06f, 193.3187f},
		{-81.80939f, 0f, 192.9302f},
		{-90f, 192.9109f, 0f},
		{-77.74829f, 180f, 12.82869f},
		{-65.12466f, 180f, 12.47113f},
		{-51.21573f, 180f, 12.21864f},
		{-35.50565f, 180f, 12.34389f},
		{-20.58813f, 180f, 11.82933f},
		{-6.791473f, 180f, 11.11929f},
		{2.761263f, 180f, 11.19223f},
		{6.599105f, 180f, 11.34655f},
		{4.547576f, 180f, 11.18934f},
		{-5.63205f, 180f, 11.40351f},
		{-16.52155f, 180f, 12.92032f},
		{-24.97095f, 180f, 14.25559f},
		{-30.90417f, 180f, 15.09315f},
		{-33.04413f, 180f, 14.8538f},
		{-33.0379f, 180f, 14.73635f},
		{-29.83746f, 180f, 13.75328f},
		{-25.06332f, 180f, 12.69873f},
		{-18.68576f, 180f, 11.77816f},
		{-13.65735f, 180f, 11.26403f},
		{-9.964386f, 180f, 11.00369f},
		{-10.53793f, 180f, 11.00549f},
		{-15.44406f, 180f, 11.34427f},
		{-24.28928f, 180f, 11.4497f},
		{-37.25793f, 180f, 10.59266f},
		{-49.87006f, 180f, 9.902032f},
		{-62.11752f, 180f, 9.939218f}};


        spine_rot_run = new float[,]{
		{-4.425171f, -8.768494f, 2.615265f},
		{-4.985413f, -6.640045f, 1.921887f},
		{-7.29068f, -7.372589f, 1.146878f},
		{-6.003571f, -1.378906f, -0.5171814f},
		{-6.602051f, 1.485064f, -1.427094f},
		{-7.616302f, 4.396369f, -1.450867f},
		{-8.506378f, 6.911948f, -0.7937012f},
		{-9.223267f, 8.843612f, 0.8481577f},
		{-9.390961f, 10.34404f, 2.334053f},
		{-8.650909f, 11.38987f, 3.440974f},
		{-7.561188f, 12.32281f, 3.701695f},
		{-6.107697f, 13.21603f, 2.669686f},
		{-4.879517f, 14.10822f, 1.287898f},
		{-4.134888f, 15.14571f, -0.3225708f},
		{-3.879639f, 15.93105f, -1.542999f},
		{-4.395233f, 16.43191f, -2.012756f},
		{-4.948486f, 16.26181f, -2.071075f},
		{-5.233185f, 15.13987f, -1.821991f},
		{-5.309845f, 13.46843f, -1.007294f},
		{-4.890411f, 11.3159f, 0.4645297f},
		{-4.64267f, 8.851368f, 2.380875f},
		{-4.894165f, 6.164771f, 5.161206f},
		{-5.216705f, 3.344922f, 7.358902f},
		{-6.241302f, -0.5736694f, 7.979296f},
		{-5.935822f, -2.701599f, 8.011942f},
		{-6.518433f, -4.816101f, 7.252981f},
		{-5.100403f, -8.060089f, 6.375144f},
		{-4.160034f, -9.899719f, 5.056049f},
		{-3.194336f, -11.04288f, 4.238428f},
		{-2.153717f, -11.4425f, 3.987262f},
		{-1.462372f, -11.72632f, 3.91346f},
		{-1.108704f, -11.89136f, 4.032985f},
		{-1.284332f, -11.96426f, 4.038691f},
		{-2.004578f, -11.96057f, 3.929682f},
		{-2.846344f, -11.35861f, 3.632791f},
		{-3.786194f, -10.12704f, 3.121267f}};


        left_arm_rot_run = new float[,]{
		{40.45017f, 180f, -29.99161f},
		{33.39113f, 180f, -29.07156f},
		{25.79508f, 180f, -27.97687f},
		{17.49292f, 180f, -26.5715f},
		{9.884659f, 180f, -24.90146f},
		{3.172167f, 180f, -23.19443f},
		{-1.781555f, 180f, -21.50262f},
		{-4.559143f, 180f, -20.01361f},
		{-6.155273f, 180f, -19.241f},
		{-6.735077f, 180f, -19.36426f},
		{-6.995087f, 180f, -20.37402f},
		{-7.237762f, 180f, -22.47983f},
		{-7.235352f, 180f, -24.82739f},
		{-7.124817f, 180f, -27.32993f},
		{-6.2117f, 180f, -29.0325f},
		{-4.200134f, 180f, -29.4306f},
		{-1.272522f, 180f, -29.01688f},
		{2.795728f, 180f, -27.94131f},
		{6.959398f, 180f, -26.73956f},
		{10.81022f, 180f, -25.20251f},
		{13.77285f, 180f, -23.61514f},
		{15.17801f, 180f, -21.86542f},
		{16.62293f, 180f, -20.95929f},
		{19.18656f, 180f, -21.76221f},
		{23.27736f, 180f, -23.42966f},
		{28.86132f, 180f, -25.38467f},
		{35.89167f, 180f, -28.38791f},
		{43.5341f, 180f, -30.90649f},
		{50.2763f, 180f, -32.59528f},
		{56.39268f, 180f, -34.18546f},
		{60.12738f, 180f, -34.7966f},
		{61.26719f, 180f, -34.71927f},
		{59.92665f, 180f, -34.22174f},
		{56.15284f, 180f, -33.13583f},
		{51.3872f, 180f, -32.09665f},
		{45.63905f, 180f, -30.86612f}};


        right_arm_rot_run = new float[,]{
		{10.26496f, 180f, 23.05305f},
		{12.60229f, 180f, 22.00208f},
		{15.76525f, 180f, 21.14517f},
		{18.45053f, 180f, 20.44392f},
		{24.84062f, 180f, 20.24279f},
		{26.22013f, 180f, 20.59158f},
		{28.85869f, 180f, 21.66725f},
		{33.21492f, 180f, 23.49733f},
		{38.38713f, 180f, 25.54827f},
		{44.34723f, 180f, 27.72737f},
		{50.25832f, 180f, 29.60996f},
		{55.94095f, 180f, 31.36142f},
		{60.44443f, 180f, 32.32028f},
		{63.20397f, 180f, 32.47229f},
		{64.54894f, 180f, 32.37742f},
		{64.51742f, 180f, 32.18837f},
		{63.32788f, 180f, 32.08172f},
		{61.09356f, 180f, 32.14549f},
		{57.73745f, 180f, 32.21313f},
		{53.27951f, 180f, 32.11967f},
		{48.03975f, 180f, 32.14221f},
		{41.29126f, 180f, 31.68366f},
		{34.67339f, 180f, 30.67307f},
		{28.61954f, 180f, 29.16129f},
		{23.4782f, 180f, 27.91631f},
		{18.89606f, 180f, 26.86343f},
		{15.18061f, 180f, 25.60697f},
		{12.76097f, 180f, 25.16604f},
		{10.29706f, 180f, 25.24881f},
		{7.78686f, 180f, 25.87038f},
		{5.2183f, 180f, 26.48598f},
		{2.54368f, 180f, 26.98535f},
		{0.36068f, 180f, 26.8449f},
		{2.66089f, 180f, 26.08421f},
		{5.92419f, 180f, 25.19593f},
		{7.08429f, 180f, 23.98077f}};


        leftfore_arm_rot_run = new float[,]{
		{57.40641f, 180f, -9.921143f},
		{53.80845f, 180f, -12.14615f},
		{50.19518f, 180f, -14.56915f},
		{46.32088f, 180f, -17.30527f},
		{44.76162f, 180f, -19.01389f},
		{46.47159f, 180f, -19.69986f},
		{51.40056f, 180f, -19.621f},
		{60.04783f, 180f, -17.85141f},
		{69.81434f, 180f, -15.53265f},
		{80.16063f, 180f, -13.57022f},
		{90f, 180f, -12f},
		{97.68255f, 180f, -11f},
		{102.14801f, 180f, -10f},
		{107.06641f, 180f, -8f},
		{111.65587f, 180f, -7f},
		{115.57528f, 180f, -8f},
		{111.55646f, 180f, -10f},
		{107.69394f, 180f, -11f},
		{102.77066f, 180f, -12f},
		{97.9904f, 180f, -13.5f},
		{90f, 180f, -15.5f},
		{75.87556f, 180f, -18.13571f},
		{62.23927f, 180f, -16.83435f},
		{52.88618f, 180f, -16.61563f},
		{48.57156f, 180f, -16.24039f},
		{48.70536f, 180f, -15.36765f},
		{50.45186f, 180f, -15.68829f},
		{54.34988f, 180f, -14.8233f},
		{57.7579f, 180f, -13.6203f},
		{60.69931f, 180f, -11.92059f},
		{62.73273f, 180f, -10.09875f},
		{63.85788f, 180f, -8.48764f},
		{63.9655f, 180f, -7.697968f},
		{63.08407f, 180f, -7.648987f},
		{61.62513f, 180f, -8.006958f},
		{59.59244f, 180f, -8.724854f}};


        rightfore_arm_rot_run = new float[,]{
		{81.71352f, 180f, 8.245888f},
		{71.4501f, 180f, 9.755672f},
		{60.38136f, 180f, 10.84572f},
		{47.92963f, 180f, 11.08745f},
		{38.03064f, 180f, 10.52183f},
		{31.9405f, 180f, 9.924243f},
		{28.676f, 180f, 9.83026f},
		{28.34454f, 180f, 10.73344f},
		{29.69238f, 180f, 11.76314f},
		{32.5424f, 180f, 12.53324f},
		{36.20818f, 180f, 12.60738f},
		{40.54676f, 180f, 11.56618f},
		{44.37115f, 180f, 9.608171f},
		{46.901f, 180f, 6.868908f},
		{49.14721f, 180f, 4.471852f},
		{51.56062f, 180f, 2.928122f},
		{54.03521f, 180f, 2.054826f},
		{56.7288f, 180f, 1.927443f},
		{59.11195f, 180f, 2.404478f},
		{60.87423f, 180f, 3.433911f},
		{62.29269f, 180f, 4.591887f},
		{62.992f, 180f, 6.257362f},
		{64.66143f, 180f, 7.744128f},
		{68.89263f, 180f, 8.780901f},
		{72.83073f, 180f, 9.145492f},
		{78.5222f, 180f, 8.965611f},
		{84.82631f, 180f, 6.976135f},
		{90f, 180f, 0f},
		{95.88834f, 180f, 7.2361f},
		{100.87695f, 180f, 7.1485f},
		{105.89034f, 180f, 7.4039f},
		{110.91074f, 180f, 7.0195f},
		{105.18093f, 180f, 7.2087f},
		{100.75531f, 180f, 7.0117f},
		{95.24508f, 180f, 7.2363f},
		{90f, 180f, 0f}};

        neck_run = new float[,]{
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f},
        {0f ,0f ,0f}};
    }

    public void get_next_movement(out Quaternion [] rot)
    {
        rot = new Quaternion[GenerateBone.TOTAL_PART];
        for (int i = 0; i < rot.Length; i++)
            rot[i] = Quaternion.identity;
        if (++step >= spine_rot_run.GetLength(0))
            step = 0;
        rot[GenerateBone.HIP].eulerAngles = new Vector3(hip_rot_run[step,0], hip_rot_run[step,1], hip_rot_run[step,2]);
        rot[GenerateBone.LEFTUP_LEG].eulerAngles = new Vector3(leftup_leg_rot_run[step, 0], leftup_leg_rot_run[step, 1], leftup_leg_rot_run[step, 2]);
        rot[GenerateBone.RIGHTUP_LEG].eulerAngles = new Vector3(rightup_leg_rot_run[step, 0], rightup_leg_rot_run[step, 1], rightup_leg_rot_run[step, 2]);
        rot[GenerateBone.LEFT_LEG].eulerAngles = new Vector3(left_leg_rot_run[step, 0], left_leg_rot_run[step, 1], left_leg_rot_run[step, 2]);
        rot[GenerateBone.RIGHT_LEG].eulerAngles = new Vector3(right_leg_rot_run[step, 0], right_leg_rot_run[step, 1], right_leg_rot_run[step, 2]);
        rot[GenerateBone.SPINE].eulerAngles = new Vector3(spine_rot_run[step, 0], spine_rot_run[step, 1], spine_rot_run[step, 2]);
        rot[GenerateBone.LEFT_ARM].eulerAngles = new Vector3(left_arm_rot_run[step, 0], left_arm_rot_run[step, 1], left_arm_rot_run[step, 2]);
        rot[GenerateBone.RIGHT_ARM].eulerAngles = new Vector3(right_arm_rot_run[step, 0], right_arm_rot_run[step, 1], right_arm_rot_run[step, 2]);
        rot[GenerateBone.LEFTFORE_ARM].eulerAngles = new Vector3(leftfore_arm_rot_run[step, 0], leftfore_arm_rot_run[step, 1], leftfore_arm_rot_run[step, 2]);
        rot[GenerateBone.RIGHTFORE_ARM].eulerAngles = new Vector3(rightfore_arm_rot_run[step, 0], rightfore_arm_rot_run[step, 1], rightfore_arm_rot_run[step, 2]);
    }

    public AnimationClip create_running()
    {
        AnimationClip clip = new AnimationClip();

        for (int i = 0; i < GenerateBone.VALID_PART; i++)
        {
            AnimationCurve curve_w = new AnimationCurve();
            AnimationCurve curve_x = new AnimationCurve();
            AnimationCurve curve_y = new AnimationCurve();
            AnimationCurve curve_z = new AnimationCurve();
            List<Keyframe> keyframe_x = new List<Keyframe>();
            List<Keyframe> keyframe_y = new List<Keyframe>();
            List<Keyframe> keyframe_z = new List<Keyframe>();
            List<Keyframe> keyframe_w = new List<Keyframe>();
            float [,] part_move;
            switch (i)
            {
                case GenerateBone.HIP:
                    part_move = hip_rot_run;
                    break;
                case GenerateBone.LEFTUP_LEG:
                    part_move = leftup_leg_rot_run;
                    break;
                case GenerateBone.RIGHTUP_LEG:
                    part_move = rightup_leg_rot_run;
                    break;
                case GenerateBone.LEFT_LEG:
                    part_move = left_leg_rot_run;
                    break;
                case GenerateBone.RIGHT_LEG:
                    part_move = right_leg_rot_run;
                    break;
                case GenerateBone.SPINE:
                    part_move = spine_rot_run;
                    break;
                case GenerateBone.LEFT_ARM:
                    part_move = left_arm_rot_run;
                    break;
                case GenerateBone.RIGHT_ARM:
                    part_move = right_arm_rot_run;
                    break;
                case GenerateBone.LEFTFORE_ARM:
                    part_move = leftfore_arm_rot_run;
                    break;
                case GenerateBone.RIGHTFORE_ARM:
                    part_move = rightfore_arm_rot_run;
                    break;
                default:
                    part_move = neck_run;
                    break;
            }
            for (int j = 0; j < part_move.GetLength(0); j++)
            {
                Quaternion rot = Quaternion.identity;
                rot.eulerAngles = new Vector3(part_move[j, 0], part_move[j, 1], part_move[j, 2]);
                keyframe_w.Add(new Keyframe(j / 60f, rot.w));
                keyframe_x.Add(new Keyframe(j / 60f, rot.x));
                keyframe_y.Add(new Keyframe(j / 60f, rot.y));
                keyframe_z.Add(new Keyframe(j / 60f, rot.z));
            }
            curve_w.keys = keyframe_w.ToArray();
            clip.SetCurve(GenerateBone.RelativeBoneName[i], typeof(Transform), "m_LocalRotation.w", curve_w);
            curve_x.keys = keyframe_x.ToArray();
            clip.SetCurve(GenerateBone.RelativeBoneName[i], typeof(Transform), "m_LocalRotation.x", curve_x);
            curve_y.keys = keyframe_y.ToArray();
            clip.SetCurve(GenerateBone.RelativeBoneName[i], typeof(Transform), "m_LocalRotation.y", curve_y);
            curve_z.keys = keyframe_z.ToArray();
            clip.SetCurve(GenerateBone.RelativeBoneName[i], typeof(Transform), "m_LocalRotation.z", curve_z);
            clip.wrapMode = WrapMode.Loop;            
        }
        return clip;
    }
}
