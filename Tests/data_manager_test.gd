# GdUnit generated TestSuite
class_name DataManagerTest
extends GdUnitTestSuite
@warning_ignore('unused_parameter')
@warning_ignore('return_value_discarded')

# TestSuite generated from
const __source = 'res://Core/Main/data_manager.gd'

func test_get_singleton() -> void:
	var data_manager = DataManager.new()
	add_child(data_manager)
	assert_object(data_manager.get_singleton()).is_class("DataManager")

func test_SET_GET_HAS() -> void:
	var DM = DataManager.get_singleton()
	assert_bool(DM.HAS("")).is_false()
	assert_bool(DM.HAS("unit_test/testGet")).is_false()
	assert_str(DM.SET("unit_test/testGet", "Hi!")).is_equal("Hi!")
	assert_bool(DM.HAS("unit_test/testGet")).is_true()
	assert_str(DM.GET("unit_test/testGet")).is_equal("Hi!")

