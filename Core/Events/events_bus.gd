class_name EventsBus

class EVENTS_Item:
	static var new_item: Signal = Utils.make_static_signal(EVENTS_Item, 'new_item')
	static var delete_item: Signal = Utils.make_static_signal(EVENTS_Item, 'delete_item')
