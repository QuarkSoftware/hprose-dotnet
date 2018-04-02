﻿/**********************************************************\
|                                                          |
|                          hprose                          |
|                                                          |
| Official WebSite: http://www.hprose.com/                 |
|                   http://www.hprose.org/                 |
|                                                          |
\**********************************************************/
/**********************************************************\
 *                                                        *
 * MembersAccessor.cs                                     *
 *                                                        *
 * MembersAccessor class for C#.                          *
 *                                                        *
 * LastModified: Apr 2, 2018                              *
 * Author: Ma Bingyao <andot@hprose.com>                  *
 *                                                        *
\**********************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using static System.Reflection.BindingFlags;

namespace Hprose.IO.Accessors {
    internal static class MembersAccessor {
        public static IReadOnlyDictionary<string, MemberInfo> GetMembers(Type type) {
            var members = new Dictionary<string, MemberInfo>(StringComparer.OrdinalIgnoreCase);
            var flags = Public | Instance;
            var isDataContract = type.IsDefined(typeof(DataContractAttribute), false);
            if (isDataContract) {
                flags |= NonPublic;
            }
            var properties = type.GetProperties(flags);
            var ignoreDataMember = typeof(IgnoreDataMemberAttribute);
            string name;
            foreach (var property in properties) {
                var dataMember = property.GetCustomAttribute<DataMemberAttribute>(false);
                if (property.CanRead && property.CanWrite &&
                    (!isDataContract || dataMember != null) &&
                    !property.IsDefined(ignoreDataMember, false) &&
                    property.GetIndexParameters().Length == 0 &&
                    !members.ContainsKey(name = dataMember?.Name ?? property.Name)) {
                    name = char.ToLower(name[0]) + name.Substring(1);
                    members[name] = property;
                }
            }
            var fields = type.GetFields(flags);
            foreach (var field in fields) {
                var dataMember = field.GetCustomAttribute<DataMemberAttribute>(false);
                if ((!isDataContract || dataMember != null) &&
                    !field.IsDefined(ignoreDataMember, false) &&
                    !field.IsNotSerialized &&
                    !members.ContainsKey(name = dataMember?.Name ?? field.Name)) {
                    name = char.ToLower(name[0]) + name.Substring(1);
                    members[name] = field;
                }
            }
            return (from entry in members
                    orderby entry.Value.GetCustomAttribute<DataMemberAttribute>(false)?.Order ?? 0
                    select entry).ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value,
                        StringComparer.OrdinalIgnoreCase
                    );
        }
    }
    public static class MembersAccessor<T> {
        public static readonly IReadOnlyDictionary<string, MemberInfo> Members;
        static MembersAccessor() {
            Members = MembersAccessor.GetMembers(typeof(T));
        }
    }
}
