namespace Microsoft.Deployment.Actions.SalesforceADF.Models
{
    public class DeserializedADFPayload
    {
        public Field[] fields;
    }

    public class Field
    {
        public string Item1;
        public Item2[] Item2;
    }

    public class Item2
    {
        public string name;
        public string type;
    }

}
