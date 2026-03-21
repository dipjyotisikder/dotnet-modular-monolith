namespace Shared.Domain.Authorization;

public static class Permission
{
    public const string UserCreate = "user.create";
    public const string UserRead = "user.read";
    public const string UserUpdate = "user.update";
    public const string UserDelete = "user.delete";
    public const string UserListAll = "user.list_all";
    public const string UserManageRoles = "user.manage_roles";
    public const string UserManageTier = "user.manage_tier";
    public const string UserDeactivate = "user.deactivate";

    public const string HotelCreate = "hotel.create";
    public const string HotelRead = "hotel.read";
    public const string HotelUpdate = "hotel.update";
    public const string HotelDelete = "hotel.delete";
    public const string HotelListAll = "hotel.list_all";
    public const string HotelAddRoom = "hotel.add_room";
    public const string HotelManageRooms = "hotel.manage_rooms";

    public const string RoomCreate = "room.create";
    public const string RoomRead = "room.read";
    public const string RoomUpdate = "room.update";
    public const string RoomDelete = "room.delete";

    public const string BookingCreate = "booking.create";
    public const string BookingRead = "booking.read";
    public const string BookingUpdate = "booking.update";
    public const string BookingCancel = "booking.cancel";
    public const string BookingComplete = "booking.complete";
    public const string BookingViewAll = "booking.view_all";
    public const string BookingSearch = "booking.search";

    public const string DeviceViewOwn = "device.view_own";
    public const string DeviceRevokeOwn = "device.revoke_own";
    public const string DeviceRevokeAll = "device.revoke_all";
    public const string DeviceViewAll = "device.view_all";

    public static IReadOnlyList<string> GetAllPermissions() =>
    [
        UserCreate, UserRead, UserUpdate, UserDelete, UserListAll,
        UserManageRoles, UserManageTier, UserDeactivate,

        HotelCreate, HotelRead, HotelUpdate, HotelDelete, HotelListAll,
        HotelAddRoom, HotelManageRooms,

        RoomCreate, RoomRead, RoomUpdate, RoomDelete,

        BookingCreate, BookingRead, BookingUpdate, BookingCancel,
        BookingComplete, BookingViewAll, BookingSearch,

        DeviceViewOwn, DeviceRevokeOwn, DeviceRevokeAll, DeviceViewAll
    ];
}

public static class RolePermissions
{
    public static IReadOnlySet<string> AdminPermissions =>
        new HashSet<string>(Permission.GetAllPermissions());

    public static IReadOnlySet<string> UserPermissions => new HashSet<string>
    {
        Permission.UserRead,
        Permission.UserUpdate,
        Permission.HotelRead,
        Permission.HotelListAll,
        Permission.BookingCreate,
        Permission.BookingRead,
        Permission.BookingCancel,
        Permission.BookingSearch,
        Permission.DeviceViewOwn,
        Permission.DeviceRevokeOwn,
        Permission.DeviceRevokeAll
    };

    public static IReadOnlySet<string> HotelManagerPermissions => new HashSet<string>
    {
        Permission.UserRead,
        Permission.HotelCreate,
        Permission.HotelRead,
        Permission.HotelUpdate,
        Permission.HotelListAll,
        Permission.HotelAddRoom,
        Permission.HotelManageRooms,
        Permission.RoomCreate,
        Permission.RoomRead,
        Permission.RoomUpdate,
        Permission.RoomDelete,
        Permission.BookingViewAll,
        Permission.BookingRead,
        Permission.BookingComplete,
        Permission.BookingCancel,
        Permission.DeviceViewOwn,
        Permission.DeviceRevokeOwn
    };

    public static IReadOnlySet<string> GuestPermissions => new HashSet<string>
    {
        Permission.HotelRead,
        Permission.HotelListAll
    };

    public static IReadOnlySet<string> GetPermissionsForRole(string role) => role.ToLowerInvariant() switch
    {
        "admin" => AdminPermissions,
        "user" => UserPermissions,
        "hotelmanager" => HotelManagerPermissions,
        "guest" => GuestPermissions,
        _ => GuestPermissions
    };
}
