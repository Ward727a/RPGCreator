extends Button


func _pressed():
	
	var folders: Dictionary = FolderManager.check_engine_folders()
	
	if folders['not_found'].size() != 0:
		print("Their is some not found folder...")
		
		for folder in folders['not_found']:
			print(folder, " not found.")
		
		print("Creating them...")
		
		if !FolderManager.create_missing_folders(folders):
			print("ERROR: COULDN'T CREATE THE MISSING FOLDERS!!!")
			return
		
		print ("Created the missing folder with success.")
