using System;
using System.Collections.Generic;
using System.IO;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Statics
{
    public class PathHelper
    {
        public static string GetPath(PATH path, Dictionary<string, string> dirs, bool issystempath)
        {
            string nd = Path.DirectorySeparatorChar.ToString();
            string mappath = System.Web.HttpContext.Current == null ? "c:" + nd : System.Web.HttpContext.Current.Server.MapPath("~");
            string systempath = issystempath ? mappath : nd;
            dirs["identity_id"] = dirs.ContainsKey("identity_id") ? Sanitize(dirs["identity_id"]) : "";
            dirs["booth_sysname"] = dirs.ContainsKey("booth_sysname") ? Sanitize(dirs["booth_sysname"]) : "";
            dirs["product_sysname"] = dirs.ContainsKey("product_sysname") ? Sanitize(dirs["product_sysname"]) : "";
            dirs["collection_sysname"] = dirs.ContainsKey("collection_sysname") ? Sanitize(dirs["collection_sysname"]) : "";

            string return_path = "";
            switch (path)
            {
                /*case PATH.PROFILE_DIRECTORY:
                    if (!string.IsNullOrEmpty(dirs["username"]))
                        return_path = systempath + "images" + nd + "profiles" + nd;
                    else
                        throw new Exception();
                    break;*/
                case PATH.PROFILE_DIRECTORY_NAME:
                    if (!string.IsNullOrEmpty(dirs["identity_id"]))
                        return_path = systempath + "_content\\images" + nd + "profiles" + nd + dirs["identity_id"] + nd + "profile" + nd;
                    else
                        throw new Exception("A-OK, handled.");
                    break;
                case PATH.PROFILE_DIRECTORY_TMP:
                    if (!string.IsNullOrEmpty(dirs["identity_id"]))
                        return_path = systempath + "_content\\images" + nd + "profiles" + nd + dirs["identity_id"] + nd + "tmp_profile" + nd;
                    else
                        throw new Exception("A-OK, handled.");
                    break;
                case PATH.BOOTH_DIRECTORY:
                    if (!string.IsNullOrEmpty(dirs["identity_id"]))
                        return_path = systempath + "_content\\images" + nd + "profiles" + nd + dirs["identity_id"] + nd + "booths" + nd;
                    else
                        throw new Exception("A-OK, handled.");
                    break;
                case PATH.BOOTH_DIRECTORY_NAME:
                    if (!string.IsNullOrEmpty(dirs["identity_id"]) && !string.IsNullOrEmpty(dirs["booth_sysname"]))
                        return_path = systempath + "_content\\images" + nd + "profiles" + nd + dirs["identity_id"] + nd + "booths" + nd + dirs["booth_sysname"] + nd;
                    else
                        throw new Exception("A-OK, handled.");
                    break;
                case PATH.BOOTH_DIRECTORY_TMP:
                    if (!string.IsNullOrEmpty(dirs["identity_id"]))
                        return_path = systempath + "_content\\images" + nd + "profiles" + nd + dirs["identity_id"] + nd + "tmp_booths" + nd;
                    else
                        throw new Exception("A-OK, handled.");
                    break;
                case PATH.PRODUCT_DIRECTORY:
                    if (!string.IsNullOrEmpty(dirs["identity_id"]) && !string.IsNullOrEmpty(dirs["booth_sysname"]))
                        return_path = systempath + "_content\\images" + nd + "profiles" + nd + dirs["identity_id"] + nd + "booths" + nd + dirs["booth_sysname"] + nd + "products" + nd;
                    else
                        throw new Exception("A-OK, handled.");
                    break;
                case PATH.PRODUCT_DIRECTORY_NAME:
                    if (!string.IsNullOrEmpty(dirs["identity_id"]) && !string.IsNullOrEmpty(dirs["booth_sysname"]) && !string.IsNullOrEmpty(dirs["product_sysname"]))
                        return_path = systempath + "_content\\images" + nd + "profiles" + nd + dirs["identity_id"] + nd + "booths" + nd + dirs["booth_sysname"] + nd + "products" + nd + dirs["product_sysname"] + nd;
                    else
                        throw new Exception("A-OK, handled.");
                    break;
                case PATH.PRODUCT_DIRECTORY_TMP:
                    if (!string.IsNullOrEmpty(dirs["identity_id"]))
                        return_path = systempath + "_content\\images" + nd + "profiles" + nd + dirs["identity_id"] + nd + "tmp_products" + nd;
                    else
                        throw new Exception("A-OK, handled.");
                    break;
                case PATH.COLLECTION_DIRECTORY:
                    if (!string.IsNullOrEmpty(dirs["identity_id"]) && !string.IsNullOrEmpty(dirs["booth_sysname"]))
                        return_path = systempath + "_content\\images" + nd + "profiles" + nd + dirs["identity_id"] + nd + "booths" + nd + dirs["booth_sysname"] + nd + "collections" + nd;
                    else
                        throw new Exception("A-OK, handled.");
                    break;
                case PATH.COLLECTION_DIRECTORY_NAME:
                    if (!string.IsNullOrEmpty(dirs["identity_id"]) && !string.IsNullOrEmpty(dirs["booth_sysname"]) && !string.IsNullOrEmpty(dirs["collection_sysname"]))
                        return_path = systempath + "_content\\images" + nd + "profiles" + nd + dirs["identity_id"] + nd + "booths" + nd + dirs["booth_sysname"] + nd + "collections" + nd + dirs["collection_sysname"] + nd;
                    else
                        throw new Exception("A-OK, handled.");
                    break;
                case PATH.COLLECTION_DIRECTORY_TMP:
                    if (!string.IsNullOrEmpty(dirs["identity_id"]))
                        return_path = systempath + "_content\\images" + nd + "profiles" + nd + dirs["identity_id"] + nd + "tmp_collections" + nd;
                    else
                        throw new Exception("A-OK, handled.");
                    break;
                default:
                    throw new Exception("A-OK, handled.");
            }
            //if(CreatePath(return_path))
                return return_path;
            //return "";
        }
        public static void ClearFolder(string FolderName, bool clear_own, bool clear_folders)
        {
            if (System.IO.Directory.Exists(FolderName))
            {
                DirectoryInfo dir = new DirectoryInfo(FolderName);

                foreach (FileInfo fi in dir.GetFiles())
                {
                    fi.Delete();
                }

                if (clear_folders)
                {
                    foreach (DirectoryInfo di in dir.GetDirectories())
                    {
                        ClearFolder(di.FullName, true, true);
                        //di.Delete();
                    }
                }
                if (clear_own)
                    dir.Delete();
            }
        }
        private static string Sanitize(string s)
        {
            foreach (char invalidchar in System.IO.Path.GetInvalidFileNameChars())
            {
                s = s.Replace(invalidchar, '_');
            }
            return s;
        }
        public static bool CreatePath(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }
            return false;
        }
        public static void MoveFile(string src_dir, string old_name, string dest_dir, string new_name, bool delete_orig, bool clear_newpath, bool clear_oldpath, bool clean_newpath_folders)
        {
            // Use static Path methods to extract only the file name from the path.
            PathHelper.CreatePath(dest_dir);
            if (clear_newpath)
                ClearFolder(dest_dir, false, clean_newpath_folders);
            string srcFile = System.IO.Path.Combine(src_dir, Path.GetFileName(old_name));
            string destFile = System.IO.Path.Combine(dest_dir, Path.GetFileName(new_name));
            System.IO.File.Copy(srcFile, destFile, true);
            if (delete_orig)
                DeleteFile(src_dir, old_name, clear_oldpath);
        }
        public static void DeleteFile(string path, string file, bool clearpath)
        {
            System.IO.File.Delete(path + file);

            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0)
                ClearFolder(path, true, true);
            if (clearpath)
                ClearFolder(path, true, true);
        }
        public static List<string> GetFileNames(PATH dir_tmp, Dictionary<string, string> dirs, bool fullpath)
        {
            string tmp_dir = PathHelper.GetPath(dir_tmp, dirs, true);
            CreatePath(tmp_dir);
            if (System.IO.Directory.Exists(tmp_dir))
            {
                List<string> fnames = new List<string>();
                string[] files = System.IO.Directory.GetFiles(tmp_dir);
                foreach (string fname in files)
                {
                    if (fullpath)
                        fnames.Add(fname);
                    else
                        fnames.Add(Path.GetFileName(fname));
                }
                return fnames;
            }
            else
                throw new Exception("A-OK, Check.");
        }
        public static string GenerateFileName(string context, FILE_NAME hashOrGuidOrNone)
        {
            string result = "";
            int len = context.Length > 8 ? 8 : context.Length;
            switch (hashOrGuidOrNone)
            {
                case FILE_NAME.HASH:
                    result = "HASH_" + SecurityHelper.GenerateHashSHA(context.Substring(context.Length - len, len)) + "_{" + DateTime.Now.ToString("yyyyMMdd-HHmmssfff") + "}";
                    break;
                case FILE_NAME.GUID:
                    result = "GUID_" + Guid.NewGuid().ToString("B") + "_{" + DateTime.Now.ToString("yyyyMMdd-HHmmssfff") + "}";
                    break;
                case FILE_NAME.NONE:
                    result = "NONE_{" + DateTime.Now.ToString("yyyyMMdd-HHmmssfff") + "}";
                    break;
            }
            return result;
        }
        public static string GenerateFolderName(PATH path, Dictionary<string, string> dirs, string identifier/*, FILE_NAME hashOrGuidOrNone*/)
        {
            string res_path = "";
            string systempath = "";
            while (string.IsNullOrEmpty(systempath))
            {
                //switch (hashOrGuidOrNone)
                //{
                //case FILE_NAME.HASH:
                //    res_path = "HASH_" + identifier + "_" + GenerateHash(path);
                //    break;
                //case FILE_NAME.GUID:
                //res_path = "GUID_" + identifier + "_" + Guid.NewGuid().ToString("D");//B - braces
                //    if (dirs.Count == 0)
                //        dirs["identity_id"] = res_path;
                //    break;
                //case FILE_NAME.NONE:
                res_path = identifier + "_{" + DateTime.Now.ToString("yyyyMMdd-HHmmssfff") + "}";
                if (dirs.Count == 0)
                    dirs["identity_id"] = res_path;
                //break;
                //}
                systempath = PathHelper.GetPath(path, dirs, true);
            }

            //if()
            CreatePath(systempath);
            return res_path;
            //throw new Exception();            
        }
    }
}