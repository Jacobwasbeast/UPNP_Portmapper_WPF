namespace Portmapper;

public class PortMapping
{
    public string Name { get; set; }
    public int InternalPort { get; set; }
    public int ExternalPort { get; set; }
    public string Protocol { get; set; }
    public string IPAddress { get; set; }
}