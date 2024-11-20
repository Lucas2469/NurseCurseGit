using System.Data;

namespace NurseCourse.Models
{
    public class VUser
    {
        public VUser()
        {
            Roles = new HashSet<Role>();
            AssignedRoleIds = new HashSet<string>();
            identities = new List<Identity>();
        }

        public string user_id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string nickname { get; set; }
        public string picture { get; set; }
        public string last_login { get; set; }
        public int logins_count { get; set; }
        public bool blocked { get; set; }

        // Nueva propiedad para las identidades
        public List<Identity> identities { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public ICollection<string> AssignedRoleIds { get; set; }
    }

    public class Identity
    {
        public string connection { get; set; }
        public string Provider { get; set; }
        public string UserId { get; set; }
    }
}
