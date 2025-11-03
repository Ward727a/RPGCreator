import os
import json
import sys

def load_local_description(path):
    """Load a local @description.json file if it exists."""
    desc_path = os.path.join(path, "@description.json")
    if os.path.exists(desc_path):
        try:
            with open(desc_path, "r", encoding="utf-8") as f:
                data = json.load(f)
                if not isinstance(data, dict):
                    return {}
                return {
                    "description": data.get("description", ""),
                    "ignore_dir": data.get("ignore_dir", [])
                }
        except Exception as e:
            print(f"Warning: Failed to parse {desc_path}: {e}")
    return {"description": "", "ignore_dir": []}


def build_tree(path, global_ignored):
    """Recursively scans folders and builds a JSON hierarchy."""
    local_data = load_local_description(path)
    local_ignore = set(local_data.get("ignore_dir", []))

    ignored = set(global_ignored or [])
    ignored.update(local_ignore)

    node = {
        "name": os.path.basename(path),
        "description": local_data.get("description", ""),
        "children": []
    }

    try:
        entries = sorted(os.listdir(path))
        for entry in entries:
            full = os.path.join(path, entry)
            if os.path.isdir(full) and entry not in ignored:
                node["children"].append(build_tree(full, global_ignored))
    except PermissionError:
        pass

    return node


if __name__ == "__main__":
    root = sys.argv[1] if len(sys.argv) > 1 else "."
    output = sys.argv[2] if len(sys.argv) > 2 else "folder_structure.json"
    global_template = sys.argv[3] if len(sys.argv) > 3 else None

    root_abs = os.path.abspath(root)
    if not os.path.exists(root_abs):
        print(f"Error: Path not found → {root_abs}")
        sys.exit(1)

    print(f"Scanning: {root_abs}")

    # Default ignored directories
    ignore_dirs = [".git", "__pycache__", ".idea", ".vscode", ".vs", ".github", "bin", "obj", ".config"]
    template_data = None

    # Load global ignore list if template provided
    if global_template and os.path.exists(global_template):
        print(f"Using global template: {global_template}")
        with open(global_template, "r", encoding="utf-8") as f:
            template_data = json.load(f)
        if "ignore_dir" in template_data and isinstance(template_data["ignore_dir"], list):
            ignore_dirs = list(set(ignore_dirs + template_data["ignore_dir"]))
            print(f"Ignoring directories (global): {ignore_dirs}")
    else:
        print("No global description template found, using default ignore list.")

    # Build folder hierarchy recursively
    data = build_tree(root_abs, global_ignored=ignore_dirs)

    # Save final JSON
    with open(output, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

    print(f"Folder hierarchy saved to {output}")
