
public static class Constants
{
    public static string PREFIX = "[GreeterMA] ";
    public static string STANDBY = PREFIX + "Press red button to capture photo";
    public static string NONE = "none";
    public const string PREFIX_TRAIN_IMAGES_PATH = "/Images/People/";
    public const string PREFIX_DETECTION_IMAGES_PATH = "/Images/Runtime/";
    public const string PREFIX_TRAIN_IMAGE_NAME = "people";
    public const string IMAGE_FORMAT = ".jpg";

    // For cases that stops with an error in the middle use these "Step codes"
    public static int AZURE_CHECK_PERSONGROUP_EXISTS = 20001;
    public static int AZURE_CREATING_PERSONGROUP = 20002;
    public static int AZURE_CHECK_PERSONGROUP_NOT_EMPTY = 20003;
    public static int AZURE_CREATING_PERSONINGROUP = 20004;
    public static int AZURE_CHECK_PERSONS_HAVE_FACES = 20005;
    public static int AZURE_DELETING_PERSONSWITHOUTFACES = 20006;
    public static int AZURE_ADDING_FACES_TO_PERSONINGROUP = 20007;
    public static int AZURE_CHECK_PERSONGROUP_TRAINED = 20008;
    public static int AZURE_PERSON_DETECTION = 20009;

    // Azure defined. Do not change this without consulting Azure documentation!
    public static string AZURE_PERSONGROUPNOTTRAINED_CODE = "PersonGroupNotTrained";
    public static string AZURE_PERSONGROUPTRAINSUCCESS = "succeeded";

    // Success
    public static string API_KEY_LOADED = PREFIX + "Api key loaded";

    // Error texts
    public static string MANAGERS_NOT_PRESENT = "One or all managers not present";
    public static string API_KEY_NOT_LOADED = PREFIX + "API Key not loaded";
    public static string CANVAS_NOT_FOUND = PREFIX + "Main canvas not found";
    public static string UI_MANAGER_NOT_FOUND = PREFIX + "UI Manager not found";

    // Status
    public static string READY_TO_DETECT = "Ready to detect";
    public static string TRYING_RECOGNITION = "Trying to recognize face";
    public static string NEED_TO_CAPTURE_FACES = "Press red button to capture faces";
    public static string REGISTERING = "Registering...";
    public static string RESTART_DETECTION_FLOW = "Please recapture";
    public static string CAPTURING = PREFIX + "Capturing";
    public static string PERSON_KNOWN = "Verified";
    public static string PERSON_UNKNOWN = "Unknown person";
}
