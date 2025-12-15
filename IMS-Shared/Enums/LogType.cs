namespace IMS_Shared.Enums;

public enum LogType
{
    Default = 0,

    Stock_Added = 1,

    [Obsolete("Use Confirmed_Purchase_Created instead.")]
    Stock_Sold = 2,

    Added_Item = 3,
    Edited_Item = 4,
    Removed_Item = 5,

    Supplier_Added = 6,
    Supplier_Order_Added = 7,
    Supplier_Order_Received = 8,
    Supplier_Order_Canceled = 9,

    Seller_Added = 10,
    Seller_Status_Changed = 11,

    Purchase_Canceled = 12,
    Confirmed_Purchase_Created = 13,
    Pending_Purchase_Created = 14,
    Pending_Purchase_Confirmed = 15,
}