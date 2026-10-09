namespace HP.GFriend.Core.Execution
{
    public class SystemVariables
    {
        /// <summary>
        /// Current repeat count in Repeat block
        /// </summary>
        public static string REPEAT_COUNT = "${R}";

        /// <summary>
        ///  Default device's address
        /// </summary>
        public static string DUT_ADDRESS = "${DUT_Address}";

        /// <summary>
        /// Default device's admin Id
        /// </summary>
        public static string DUT_ADMINID = "${DUT_AdminID}";

        /// <summary>
        /// Default device's admin Pw
        /// </summary>
        public static string DUT_ADMINPW = "${DUT_AdminPW}";

        /// <summary>
        /// Default device's Identifier
        /// </summary>
        public static string DUT_ID = "${DUT_ID}";

        /// <summary>
        /// Default device's Type
        /// </summary>
        public static string DUT_TYPE = "${DUT_TYPE}";

        /// <summary>
        /// Folder of script file is located
        /// </summary>
        public static string SCRIPT_FOLDER = "${SCRIPT_FOLDER}";

        /// <summary>
        /// Folder of output files are saved
        /// </summary>
        public static string OUTPUT_FOLDER = "${OUTPUT_FOLDER}";

        /// <summary>
        /// Last executed keyword's output
        /// </summary>
        public static string KEYWORD_OUTPUT = "${KEYWORD_OUTPUT}";


        /// <summary>
        /// Last executed keyword's result
        /// </summary>
        public static string KEYWORD_RESULT = "${KEYWORD_RESULT}";

        /// <summary>
        /// Empty string
        /// </summary>
        public static string EMPTY = "${EMPTY}";

        /// <summary>
        /// Carriage Return (\r)
        /// </summary>
        public static string CARRIAGE_RETUREN = "${CARRIAGE_RETURN}";

        /// <summary>
        /// Line Feed (\n)
        /// </summary>
        public static string LINE_FEED = "${LINE_FEED}";

        /// <summary>
        /// Current for loop iteration in for loop block
        /// </summary>
        public static string FORLOOP_ITERATION = "${ITEM}";

        /// <summary>
        /// Variable to store the test case name
        /// </summary>
        public static string TESTCASE_NAME = "${TESTCASE_NAME}";

        /// <summary>
        /// Gets the CO_DEVELOPER_MODE
        /// </summary>
        public static string CO_DEVELOPER_MODE = "${CO_DEVELOPER_MODE}";
    }

}
