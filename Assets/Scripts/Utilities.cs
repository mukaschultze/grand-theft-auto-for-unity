using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace GrandTheftAuto {
    public static class Utilities {

        private static readonly string[] bytesSizes = new string[] { "B", "KB", "MB", "GB" };

        public static Quaternion GetQuaternionFromMatrix(this Matrix4x4 matrix) {
            //http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/
            var result = new Quaternion();
            var tr = matrix.m00 + matrix.m11 + matrix.m22;

            if(tr > 0f) {
                var s = Mathf.Sqrt(tr + 1f) * 2f;
                result.w = 0.25f * s;
                result.x = (matrix.m21 - matrix.m12) / s;
                result.y = (matrix.m02 - matrix.m20) / s;
                result.z = (matrix.m10 - matrix.m01) / s;
            }
            else if(matrix.m00 > matrix.m11 && matrix.m00 > matrix.m22) {
                var s = Mathf.Sqrt(1f + matrix.m00 - matrix.m11 - matrix.m22) * 2f;
                result.w = (matrix.m21 - matrix.m12) / s;
                result.x = 0.25f * s;
                result.y = (matrix.m01 + matrix.m10) / s;
                result.z = (matrix.m02 + matrix.m20) / s;
            }
            else if(matrix.m11 > matrix.m22) {
                var s = Mathf.Sqrt(1f + matrix.m11 - matrix.m00 - matrix.m22) * 2f;
                result.w = (matrix.m02 - matrix.m20) / s;
                result.x = (matrix.m01 + matrix.m10) / s;
                result.y = 0.25f * s;
                result.z = (matrix.m12 + matrix.m21) / s;
            }
            else {
                var s = Mathf.Sqrt(1f + matrix.m22 - matrix.m00 - matrix.m11) * 2f;
                result.w = (matrix.m10 - matrix.m01) / s;
                result.x = (matrix.m02 + matrix.m20) / s;
                result.y = (matrix.m12 + matrix.m21) / s;
                result.z = 0.25f * s;
            }

            return result;
        }

        public static string GetString(this byte[] data) {
            return Encoding.ASCII.GetString(data);
        }

        public static string GetNullTerminatedString(this byte[] data) {
            var length = Array.IndexOf(data, byte.MinValue);

            if(length == -1)
                length = data.Length;

            return Encoding.ASCII.GetString(data, 0, length);
        }

        public static void SetMaterialTransparency(Material material, bool transparent) {
            if(transparent) {
                material.EnableKeyword("_ALPHA_FADE");
                material.SetInt("_ZWrite", 1);
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            else {
                material.DisableKeyword("_ALPHA_FADE");
                material.SetInt("_ZWrite", 1);
                material.SetInt("_SrcBlend", (int)BlendMode.One);
                material.SetInt("_DstBlend", (int)BlendMode.Zero);
                material.renderQueue = (int)RenderQueue.Geometry;
            }
        }

        public static void SetMaterialCulling(Material material, bool cullBack) {
            material.SetInt("_Cull", (int)(cullBack ? CullMode.Back : CullMode.Off));
        }

        public static string GetTimeFormated(this TimeSpan timeSpan) {
            return string.Format("{0:00}:{1:00}.{2:000}", timeSpan.Minutes + timeSpan.Hours * 60, timeSpan.Seconds, timeSpan.Milliseconds);
        }

        public static string GetLongTimeFormatted(this TimeSpan timeSpan) {
            if(timeSpan.TotalHours >= 1d)
                return string.Format("{0} hours", timeSpan.TotalHours.ToString("0.000"));
            else if(timeSpan.TotalMinutes >= 1d)
                return string.Format("{0} minutes", timeSpan.TotalMinutes.ToString("0.000"));
            else if(timeSpan.TotalSeconds >= 1d)
                return string.Format("{0} seconds", timeSpan.TotalSeconds.ToString("0.000"));
            else
                return string.Format("{0} milliseconds", timeSpan.TotalMilliseconds.ToString("0.000"));
        }

        public static string GetBytesFormated(double bytes) {
            //http://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
            var order = 0;

            while(bytes >= 1024 && ++order < bytesSizes.Length)
                bytes = bytes / 1024;

            return string.Format("{0:0.##}{1}", bytes, bytesSizes[order]);
        }

        public static void SetLastValue<T>(this T[] array, T value) {
            array[array.Length - 1] = value;
        }

        public static void SetLastValue<T>(this List<T> list, T value) {
            list[list.Count - 1] = value;
        }

        public static T GetLastValue<T>(this T[] array) {
            return array[array.Length - 1];
        }

        public static T GetLastValue<T>(this List<T> list) {
            return list[list.Count - 1];
        }

        public static string GetFormatedGTAName(this GtaVersion version) {
            return version.GetFormatedGTAName(false, true);
        }

        public static string GetFormatedGTAName(this GtaVersion version, bool full) {
            return version.GetFormatedGTAName(full, full);
        }

        public static string GetFormatedGTAName(this GtaVersion version, bool fullName, bool fullVersion) {
            var name = fullName ? "Grand Theft Auto: " : "GTA ";

            switch(version) {
                case GtaVersion.III:
                    return name + "III";
                case GtaVersion.ViceCity:
                    return name + (fullVersion ? "Vice City" : "VC");
                case GtaVersion.SanAndreas:
                    return name + (fullVersion ? "San Andreas" : "SA");
                case GtaVersion.Unknown:
                    return "Unknow GTA game";
                default:
                    return "Invalid GTA game";
            }
        }

    }
}
