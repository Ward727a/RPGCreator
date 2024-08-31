
enum ItemDurabilityType {
    NONE, # Dev only - should not be used in game
    CAN_BE_DAMAGED, # Item can be damaged (e.g. armor, shields, etc.) but not destroyed
    DESTROYABLE, # Item can be destroyed (e.g. weapons, armor, etc.)
    INDESTRUCTIBLE # Item cannot be destroyed (e.g. quest items)
}