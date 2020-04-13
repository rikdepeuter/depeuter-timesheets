using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Helpers
{
    public class VersionInfo
    {
        private int _major;
        private int _minor;
        private int _build;
        private int _revision;

        public int Major
        {
            get { return _major; }
            set
            {
                if(value > 999)
                {
                    throw new ArgumentOutOfRangeException("Major", value, "Value cannot be greater than 999.");
                }
                if(value < 0)
                {
                    throw new ArgumentOutOfRangeException("Major", value, "Value cannot be smaller than 0.");
                }
                _major = value;
            }
        }

        public int Minor
        {
            get { return _minor; }
            set
            {
                if(value > 999)
                {
                    throw new ArgumentOutOfRangeException("Minor", value, "Value cannot be greater than 999.");
                }
                if(value < 0)
                {
                    throw new ArgumentOutOfRangeException("Minor", value, "Value cannot be smaller than 0.");
                }
                _minor = value;
            }
        }

        public int Build
        {
            get { return _build; }
            set
            {
                if(value > 999)
                {
                    throw new ArgumentOutOfRangeException("Build", value, "Value cannot be greater than 999.");
                }
                if(value < 0)
                {
                    throw new ArgumentOutOfRangeException("Build", value, "Value cannot be smaller than 0.");
                }
                _build = value;
            }
        }

        public int Revision
        {
            get { return _revision; }
            set
            {
                if(value > 999)
                {
                    throw new ArgumentOutOfRangeException("Revision", value, "Value cannot be greater than 999.");
                }
                if(value < 0)
                {
                    throw new ArgumentOutOfRangeException("Revision", value, "Value cannot be smaller than 0.");
                }
                _revision = value;
            }
        }

        public bool IsDevelopment { get { return Major == 0; } }

        public VersionInfo(string version)
        {
            if(string.IsNullOrEmpty(version))
            {
                throw new ValidationException("Version is required");
            }

            var versionData = new int[4];
            var parts = version.Split('.');
            for(var i = 0; i < parts.Length; i++)
            {
                int res;
                if(!int.TryParse(parts[i], out res))
                {
                    throw new ValidationException("Invalid input: {0}", parts[i]);
                }
                versionData[i] = res;
            }

            Major = versionData[0];
            Minor = versionData[1];
            Build = versionData[2];
            Revision = versionData[3];
        }

        public VersionInfo(int major, int minor, int build, int revision)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Build, Revision);
        }

        public string ToTag()
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Build);
        }

        private static bool Equals(VersionInfo x1, VersionInfo x2)
        {
            if(object.Equals(x1, null) && object.Equals(x2, null))
            {
                return true;
            }
            if(!object.Equals(x1, null) && !object.Equals(x2, null))
            {
                return x1.Major == x2.Major && x1.Minor == x2.Minor && x1.Build == x2.Build && x1.Revision == x2.Revision;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return Equals(this, obj as VersionInfo);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(VersionInfo c1, VersionInfo x)
        {
            return Equals(c1, x);
        }
        public static bool operator !=(VersionInfo c1, VersionInfo x)
        {
            return !Equals(c1, x);
        }
        public static bool operator >(VersionInfo c1, VersionInfo x)
        {
            return IsGreaterThan(c1, x);
        }
        public static bool operator <(VersionInfo c1, VersionInfo x)
        {
            return IsGreaterThan(x, c1);
        }
        public static bool operator >=(VersionInfo c1, VersionInfo x)
        {
            return IsGreaterThan(c1, x) || Equals(c1, x);
        }
        public static bool operator <=(VersionInfo c1, VersionInfo x)
        {
            return IsGreaterThan(x, c1) || Equals(c1, x);
        }

        public bool IsGreaterThan(VersionInfo info)
        {
            return IsGreaterThan(this, info);
        }

        private static bool IsGreaterThan(VersionInfo x1, VersionInfo x2)
        {
            if(x1 == null && x2 == null)
            {
                return true;
            }
            if(x1 != null && x2 != null)
            {
                if(x1.Major > x2.Major)
                {
                    return true;
                }
                if(x1.Major < x2.Major)
                {
                    return false;
                }

                if(x1.Minor > x2.Minor)
                {
                    return true;
                }
                if(x1.Minor < x2.Minor)
                {
                    return false;
                }

                if(x1.Build > x2.Build)
                {
                    return true;
                }
                if(x1.Build < x2.Build)
                {
                    return false;
                }

                if(x1.Revision > x2.Revision)
                {
                    return true;
                }
            }

            return false;
        }

        public VersionInfo IncrementRevision()
        {
            Revision++;

            if(Revision == 1000)
            {
                Revision = 0;
                Build++;
            }

            if(Build == 1000)
            {
                Build = 0;
                Minor++;
            }

            if(Minor == 1000)
            {
                Minor = 0;
                Major++;
            }

            return this;
        }

        public VersionInfo IncrementBuild()
        {
            Build++;
            Revision = 0;

            if(Build == 1000)
            {
                Build = 0;
                Minor++;
            }

            if(Minor == 1000)
            {
                Minor = 0;
                Major++;
            }

            return this;
        }
    }
}
