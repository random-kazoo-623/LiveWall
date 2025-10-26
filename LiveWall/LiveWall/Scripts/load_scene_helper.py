import json
import os
import sys

current_dir = os.path.dirname(os.path.abspath(sys.argv[0]))

def main(json_path): # to be given by c# handler

    # load the json file
    json_dir = os.path.dirname(json_path)
    logging(f"dir: {json_dir}")
    root = ""
    with open(json_path) as file:
        root = json.load(file)

    # now try to searizlie them idk man
    # logging(root)
    properties = {}
    camera = root["camera"]
    general = root["general"]
    objects = root["objects"]
    try:
        version = root["version"]
    except Exception:
        version = None
    # special case for objects
    object_property = {} # dict of values
    object_list = []
    # logging(len(objects))
    count = 1
    for object in objects:
        # uh idk
        # logging(obj)
        logging(f"checking obj no {count}")
        for o in object:
            # get every property in individual object
            if type(object[o]) == list:
                logging("Nested list found, resolving...")
                new_list = ResolveSpecialProperty_List(object[o], json_dir) # nested list
                object_property[o] = new_list
            elif  type(object[o]) == dict:
                logging("Nested dict found, resolving...")
                new_dict = ResolveSpectialProperty_Dict(o, object[o], json_dir) # nested dict, current dir
                object_property[o] = new_dict
            else:
                if type(object[o]) == str and "/" in object[o] and "." in object[o] and ".json" in object[o] and "project.json" not in object[o]:
                    json_path = os.path.join(json_dir, object[o])
                    json_path = os.path.abspath(json_path)
                    logging(f"Dependency found at {json_path}, resolving...")
                    logging(object[o])
                    root = ""
                    with open(json_path) as file:
                        root = json.load(file)
                    resolved_dict = ResolveSpectialProperty_Dict(o, root, json_dir)
                    object_property[o] = resolved_dict
                else:
                    logging(f"object name {o}, values {object[o]}")
                    object_property[o] = object[o]
        
        object_list.append(object_property)
        object_property = {} 
        count += 1
    
    # now we have deserialized everything into dicts, return them
    # intentionaly left out project.json files (since that does not appear when decompressed / can copy it to the thing anyway)
    # dependencies file paths are also left out since they are for references only... + they often reference each other leading into an incremental recursive loop -> crash

    properties["camera"] = camera
    properties["general"] = general 
    properties["objects"] = object_list
    if version != None:
        properties["version"] = version
    
    return properties

    

def ResolveSpecialProperty_List(value, json_dir): # nested list
    """
    Return a heavily nested List...
    """
    # recursive function for resovling nested lists
    # basically get all the element inside the list and check if they're nested dict or list
    new_list = []
    for item in value:
        if type(item) == dict:
            logging("Nested dict found, resolving recursively...")
            resolved_dict = ResolveSpectialProperty_Dict(None, item, json_dir)
            new_list.append(resolved_dict)
        elif type(item) == list:
            logging("Nested list found, resolving recursively...")
            resolved_list = ResolveSpecialProperty_List(value[item], json_dir)
            new_list.append(resolved_list)
        else:   
            logging(f"Single element found: {item}")
            new_list.append(item)
    # return the list
    # logging(f"Returning list with elements:\n {new_list}")
    return new_list
def ResolveSpectialProperty_Dict(object, value, json_dir): # nested dict
    """
    Return a heavily nested dict...
    """
    # recursive function for resolving nested dicts
    # basically get all the dict key inside the dict and check if they're nested dict or list
    new_dict = {}
    global current_dir
    for item in value:
        if type(value[item]) == dict:
            logging("Nested dict found, resolving recursively...")
            resolved_dict = ResolveSpectialProperty_Dict(item, value[item], json_dir)
            new_dict[item] = resolved_dict
        elif type(value[item]) == list:
            logging("Nested list found, resolving recursively...")
            resolved_list = ResolveSpecialProperty_List(value[item], json_dir)
            new_dict[item] = resolved_list
        else:
            # check if text is a json file
            if type(value[item]) == str and "/" in value[item] and "." in value[item] and ".json" in value[item] and "project.json" not in value[item]:
                json_path = os.path.join(json_dir, value[item])
                json_path = os.path.abspath(json_path)
                logging(f"Dependency found at {json_path}, resolving...")
                logging(value[item])
                root = ""
                with open(json_path) as file:
                    root = json.load(file)
                resolved_dict = ResolveSpectialProperty_Dict(item, root, json_dir)
                new_dict[item] = resolved_dict
            else:
                new_dict[item] = value[item]
                logging(f"Single element found: {item}")
                logging(value[item])
    
    # return
    # logging(f"Returning dict with keys:\n{new_dict}")
    return new_dict

def logging(content):
    if not os.path.exists("logs.txt"):
        f = open("logs.txt", "x")
        f.close()
    with open("logs.txt", "a") as file:
        file.write(str(content)+'\n')

if __name__ == "__main__":
    json_path = r"C:\Users\minh\Documents\GitHub\LiveWall\LiveWall\LiveWall\bin\Debug\net8.0-windows\scenes\2156479578\scene.json"  # path from C#
    data = main(json_path)
    print(json.dumps(data))
