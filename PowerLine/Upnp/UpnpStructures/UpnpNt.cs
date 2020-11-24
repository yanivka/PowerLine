using System;
using System.Collections.Generic;
using System.Text;

namespace PowerLine.Upnp.UpnpCustomPackets
{
    public class UpnpNt
    {
        public readonly Guid id;
        public readonly string DomainName;
        public readonly string ItemType;
        public readonly string ItemVersion;
        public readonly string ItemCuston;
        public readonly NtType Type;

       
        public enum NtType
        {
            RootDevice,
            Guid,
            DeviceType,
            ServiceType,
            CustomType,
            DomainDeviceType,
            DomainServiceType,
            DomainCustomType 
        }



        private UpnpNt(Guid id, string domainName, string itemCustom, string itemType, string itemVersion, NtType type) 
        {
            this.id = id;
            ItemCuston = itemCustom;
            DomainName = domainName;
            ItemType = itemType;
            ItemVersion = itemVersion;
            Type = type;
        }

        public UpnpNt(): this(Guid.Empty,"", "", "", "", NtType.RootDevice){}
        public UpnpNt(Guid id) : this(id, "", "", "", "", NtType.Guid) { }
        public UpnpNt(string domain, string ItemType, string ItemVersion, bool IsDevice) : this(Guid.Empty, domain,"", ItemType, ItemVersion, (IsDevice) ? NtType.DomainDeviceType : NtType.DomainServiceType) { }
        public UpnpNt(string ItemType, string ItemVersion, bool IsDevice) : this(Guid.Empty, "schemas-upnp-org", "", ItemType, ItemVersion, (IsDevice) ? NtType.DeviceType : NtType.ServiceType) { }

        public UpnpNt(string domain, string customDevice, string ItemType, string ItemVersion) : this(Guid.Empty, domain, customDevice, ItemType, ItemVersion, NtType.DomainCustomType) { }
  

        public static UpnpNt Parse(string nt)
        {
            string[] ntParts = nt.Trim().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (ntParts.Length != 2 && ntParts.Length != 5) throw new Exception("Upnp invaild nt was given");
            
            if(ntParts[0].ToLower() == "upnp")
            {
                if(ntParts[1].ToLower() == "rootdevice")
                {
                    return new UpnpNt();
                }
                else
                {
                    throw new Exception("Upnp invaild nt was given (upnp started without rootdevice)");
                }
            } 
            else if(ntParts[0].ToLower() == "uuid")
            {
                if(Guid.TryParse(ntParts[1], out Guid ntId))
                {
                    return new UpnpNt(ntId);
                }
                else
                {
                    throw new Exception("Upnp invaild nt was given (uuid started without a vaild guid)");
                }
            }
            else if(ntParts[0].ToLower() == "urn")
            {
                if(ntParts[1].ToLower() == "schemas-upnp-org")
                {
                    if(ntParts[2].ToLower() == "device")
                    {
                        return new UpnpNt(ntParts[3], ntParts[4], true);
                    }
                    else if (ntParts[2].ToLower() == "service")
                    {
                        return new UpnpNt(ntParts[3], ntParts[4], false);
                    }
                    else
                    {
                        return new UpnpNt(ntParts[1], ntParts[2], ntParts[3], ntParts[4]);
                    }
                }
                else
                {
                    if (ntParts[2].ToLower() == "device")
                    {
                        return new UpnpNt(ntParts[1], ntParts[3], ntParts[4], true);
                    }
                    else if (ntParts[2].ToLower() == "service")
                    {
                        return new UpnpNt(ntParts[1], ntParts[3], ntParts[4], false);
                    }
                    else
                    {
                        return new UpnpNt(ntParts[1], ntParts[2], ntParts[3], ntParts[4]);
                    }
                }
            }
            else
            {
                throw new Exception("Upnp invaild nt was given (Unknown starter)");
            }
        }
        public override string ToString()
        {
            switch(this.Type)
            {
                case NtType.DeviceType:
                case NtType.DomainDeviceType:
                    return $"urn:{this.DomainName}:device:{this.ItemType}:{this.ItemVersion}";
                case NtType.DomainServiceType:
                case NtType.ServiceType:
                    return $"urn:{this.DomainName}:service:{this.ItemType}:{this.ItemVersion}";
                case NtType.Guid:
                    return $"uuid:{this.id.ToString()}";
                case NtType.RootDevice:
                    return "upnp:rootdevice";
                case NtType.DomainCustomType:
                case NtType.CustomType:
                    return $"urn:{this.DomainName}:{this.ItemCuston}:{this.ItemType}:{this.ItemVersion}";
                default:
                    throw new Exception("Unsupported upnp NT type");
            }
        }

        public UpnpUsn GetUsn(Guid id) => new UpnpUsn(id, this);
        public string ToServiceId(Guid customId)
        {
            switch(this.Type)
            {
                
                case NtType.DeviceType:
                case NtType.ServiceType:
                case NtType.CustomType:
                    return $"urn:upnp-org:serviceId:{customId.ToString()}";
                case NtType.DomainCustomType:
                case NtType.DomainDeviceType:
                case NtType.DomainServiceType:
                    return $"urn:{this.DomainName}:serviceId:{customId.ToString()}";
                default:
                    throw new Exception("Uanble to create service id from non typed UPnP NT");
                }
        }

        public bool Equal(UpnpNt other)
        {
            if (this.Type != other.Type) return false;
            switch (this.Type)
            {
                case NtType.RootDevice:
                    return true;
                case NtType.Guid:
                    return (other.id == this.id) ? true : false;
                case NtType.DeviceType:
                case NtType.ServiceType:
                    return (other.ItemVersion == this.ItemVersion && other.ItemType == this.ItemType) ? true : false;
                case NtType.DomainDeviceType:
                case NtType.DomainServiceType:
                    return (other.ItemVersion == this.ItemVersion && other.ItemType == this.ItemType && this.DomainName == other.DomainName) ? true : false;
                case NtType.DomainCustomType:
                    return (other.ItemVersion == this.ItemVersion && other.ItemType == this.ItemType && this.DomainName == other.DomainName && other.ItemCuston == this.ItemCuston) ? true : false;
                case NtType.CustomType:
                    return (other.ItemVersion == this.ItemVersion && other.ItemType == this.ItemType && other.ItemCuston == this.ItemCuston) ? true : false;
                default:
                    return false;
            }
            

        }

        public bool IsTypeDevice => this.Type == UpnpNt.NtType.DeviceType || this.Type == UpnpNt.NtType.DomainDeviceType;
        public bool IsTypeService => this.Type == UpnpNt.NtType.ServiceType || this.Type == UpnpNt.NtType.DomainServiceType;
        public bool IsCustomType => this.Type == UpnpNt.NtType.DomainCustomType || this.Type == UpnpNt.NtType.CustomType;

        public bool IsTyped => this.IsTypeDevice || this.IsCustomType || this.IsTypeService;
    }

}

