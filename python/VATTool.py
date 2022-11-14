bl_info = {
    "name": "VAT_Tool",
    "author": "Wang",
    "version": (0, 1),
    "blender": (3, 1, 0),
    "location": "",
    "description": "VAT tool",
    "warning": "",
    "doc_url": "",
    "category": "Generic",
}

import bpy
import bmesh
from bpy.types import Operator,Panel,PropertyGroup,AddonPreferences
from itertools import chain
import copy
import json
import os

class Anim_property(bpy.types.PropertyGroup):
    name:bpy.props.StringProperty(default="name",description='name')
    is_recode:bpy.props.BoolProperty(default=False,description='is need recoded')
    

class VAT_property(bpy.types.PropertyGroup):
    go_name:bpy.props.StringProperty(default="Armature",description='最上层父物体的名字')
    mesh_name:bpy.props.StringProperty(default="elf",description='mesh物体的名字')


    x_min:bpy.props.FloatProperty(default=-4,description ="min x")
    x_max:bpy.props.FloatProperty(default=4,description ="max x")
    y_min:bpy.props.FloatProperty(default=-4,description ="min y")
    y_max:bpy.props.FloatProperty(default=4,description ="max y")
    z_min:bpy.props.FloatProperty(default=-4,description ="min z")
    z_max:bpy.props.FloatProperty(default=4,description ="max z")

    size:bpy.props.IntProperty(default=512,description ="vat size")
    out_dir:bpy.props.StringProperty(default="C:\\Users\\vert_anim_out",description='输出文件夹')
 
 
class OT_VAT__Bake(Operator):
    bl_idname = "vat.vat_bake"
    bl_label = "bake vertex animation"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        scene = context.scene
        vat_property = scene.vat_property
        x_range = vat_property.x_max - vat_property.x_min
        y_range = vat_property.y_max - vat_property.y_min
        z_range = vat_property.z_max - vat_property.z_min

        max_verts_count = vat_property.size
        self.left_count = max_verts_count
        armature = bpy.context.scene.objects[vat_property.go_name]
        #ob = bpy.context.active_object
        ob = bpy.context.scene.objects[vat_property.mesh_name]
        me = ob.data
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.uv_texture_add() 

        bm = bmesh.from_edit_mesh(me)

        uv_layer = bm.loops.layers.uv.verify()

        #t = ob.modifiers["Armature"]


        #t.use_vertex_groups = False
        # t.ratio =(1.0*max_verts_count)/verts_len
        
        # bbbb = []
        # for i in range(len(me.vertices)):
        #     bbbb.append(copy.deepcopy(me.vertices[i].co))

        # t.use_vertex_groups = True
        # me = ob.data

        # bpy.ops.object.mode_set(mode='OBJECT')
        # t = ob.modifiers.new(name="Remesh", type='DECIMATE')
        # t.ratio =(1.0*max_verts_count)/verts_len

        # bpy.ops.object.mode_set(mode='EDIT')
        # bm = bmesh.from_edit_mesh(me)

        #bpy.ops.object.mode_set(mode='OBJECT')

        file_dir = vat_property.out_dir
        if not os.path.exists(file_dir) :
            os.makedirs(file_dir)
        
        
        # adjust uv coordinates
        for face in bm.faces:
            for loop in face.loops:
                loop_uv = loop[uv_layer]
                # use xy position of the vertex as a uv coordinate
                loop_uv.uv.x = loop.vert.index/max_verts_count + (1.0/(2*max_verts_count))
                loop_uv.uv.y = 0.5
        bmesh.update_edit_mesh(me) 

        

        verts_count = len(bm.verts)

        acts_frame_count = {}
        acts_name = {}
        acts_recode_info = {}
        json_info = {}
        json_info['size'] = max_verts_count
        json_info['info'] = acts_recode_info

        for act in bpy.data.actions:
            
            frame_count = act.frame_range[1] - act.frame_range[0] + 1.0
            frame_count = frame_count
            if frame_count > max_verts_count :
                continue
            #name = act.name.split('|')[-1]
            name = act.name
            print(name)
            acts_name[name] = act.name
            acts_frame_count[name] = frame_count

        acts_sort = []
        for name in acts_frame_count:
            index = len(acts_sort)
            for i in range(0, len(acts_sort)):
                if acts_frame_count[acts_sort[i]] < acts_frame_count[name] :
                    index = i
                    break
            acts_sort.insert(index ,name)

        acts_is_recode = []
        for i in range(0 ,len(acts_sort)):
            acts_is_recode.append(False)
    

        img_index = 0
        def isCompleted():
            for is_recode in acts_is_recode:
                if not is_recode :
                    return False
            return True

        bpy.ops.image.new

        
        while not isCompleted():
            oldImage = bpy.data.images.get("POS_"+str(img_index), None)
            oldNormalImage = bpy.data.images.get("NORMAL"+str(img_index), None)
            if oldImage:
                bpy.data.images.remove(oldImage)
            if oldNormalImage:
                bpy.data.images.remove(oldNormalImage)
            
            newImage = bpy.data.images.new("POS_"+str(img_index), max_verts_count, max_verts_count, alpha=True)
            newNormalImage = bpy.data.images.new("NORMAL"+str(img_index), max_verts_count, max_verts_count, alpha=True)

            left_start_index = 0
            self.left_count = max_verts_count
            img_pixels = []
            img_normals = []

            def bake_vat_img(self ,i):
                print(acts_name[ acts_sort[i] ])
                action = bpy.data.actions[acts_name[ acts_sort[i] ]]

                #bpy.context.object.animation_data.action = action
                armature.animation_data.action = action
                acts_is_recode[i] = True
                start = int(action.frame_range[0])
                end = int(action.frame_range[1])
                bpy.context.scene.frame_start = start
                bpy.context.scene.frame_end = end
                
                cur_frame_count = end - start + 1
                
                self.left_count = self.left_count - cur_frame_count

                acts_recode_info[acts_sort[i]] = [max_verts_count - (self.left_count + cur_frame_count) ,cur_frame_count]

                print("left_count : " + str(self.left_count))

                for x in range(start ,end + 1) :
                    bpy.context.scene.frame_set(x)
                    ob.update_from_editmode()
                    depsgraph = bpy.context.evaluated_depsgraph_get()
                    ob_eval = ob.evaluated_get(depsgraph)
                    me = ob_eval.to_mesh()
                    for a in range(max_verts_count):
                        if a >=  verts_count :
                            img_pixels.extend([0.0, 0.0, 0.0, 0.0])
                            img_normals.extend([0.0, 0.0, 0.0, 0.0])
                        else :
                            red = ((me.vertices[a].co.x)-vat_property.x_min) / x_range
                            green = ((me.vertices[a].co.y)-vat_property.y_min) / y_range
                            blue = ((me.vertices[a].co.z) - vat_property.z_min) / z_range
                            alpha = 1.0
                            img_pixels.extend([red, green, blue, alpha])

                            red = (me.vertices[a].normal.x)*0.5 +0.5
                            green = (me.vertices[a].normal.y)*0.5 +0.5
                            blue = (me.vertices[a].normal.z)*0.5 +0.5
                            alpha = 1.0
                            img_normals.extend([red, green, blue, alpha])
                            
                    ob_eval.to_mesh_clear()
            def recode(self):
                for i in range(0 ,len(acts_sort)):
                    if not acts_is_recode[i] and acts_frame_count[acts_sort[i]] < self.left_count :
                        bake_vat_img(self ,i)
                        return True
                    else :
                        print(str(acts_frame_count[acts_sort[i]]) + "++++++" + str(self.left_count))
                return False
            
            while recode(self):
                print("------")

            newImage.pixels = img_pixels
            newImage.update()
            newImage.file_format = 'OPEN_EXR'
            newImage.filepath_raw = file_dir + '\\POS_'+str(img_index) + ".exr"
            newImage.save()

            newNormalImage.pixels = img_normals
            newNormalImage.update()
            newNormalImage.file_format = 'OPEN_EXR'
            newNormalImage.filepath_raw = file_dir + '\\NORMAL_'+str(img_index) + ".exr"
            newNormalImage.save()

            img_index = img_index + 1
        
        f = open(file_dir + '\\anim_info.txt', 'w', encoding='utf-8')
        f.write(json.dumps(json_info))
        f.close()
        print("----------------FINISHED")

        bpy.ops.object.mode_set(mode='OBJECT')
        return {'FINISHED'}

class VAT_PT_Panel(bpy.types.Panel):
    """Creates a Panel in the scene context of the properties editor"""
    bl_label="VAT Panel"
    bl_idname="VAT_PT_Panel"
    bl_space_type="VIEW_3D"
    bl_region_type="UI"
    bl_category="VAT_Tool"
    

    def draw(self, context):
        layout = self.layout
        scene = context.scene
        vat_property = scene.vat_property

        layout.label(text="VAT 工具")
        row = layout.row()
        row.scale_y = 3.0
         
        layout.label(text="物体的name")
        layout.prop(vat_property,'go_name',text="name")

        layout.label(text="mesh物体的name")
        layout.prop(vat_property,'mesh_name',text="name")
        
        layout.label(text="VAT的size")
        layout.prop(vat_property,'size',text="size")

        layout.label(text="X坐标范围")
        row = layout.row()
        row.prop(vat_property, "x_min" ,text = "min X")
        row.prop(vat_property, "x_max" ,text = "max X")

        layout.label(text="y坐标范围")
        row = layout.row()
        row.prop(vat_property, "y_min" ,text = "min Y")
        row.prop(vat_property, "y_max" ,text = "max Y")

        layout.label(text="z坐标范围")
        row = layout.row()
        row.prop(vat_property, "z_min" ,text = "min Z")
        row.prop(vat_property, "z_max" ,text = "max Z")

        layout.label(text="输出文件夹")
        layout.prop(vat_property,'out_dir',text="dir")

        layout.label(text="开始生成VAT")
        row = layout.row()
        row.scale_y = 3.0
        row.operator("vat.vat_bake")


def register():
    bpy.utils.register_class(VAT_property)
    bpy.utils.register_class(Anim_property)
    bpy.types.Scene.vat_property = bpy.props.PointerProperty(type=VAT_property)
    bpy.types.Scene.anims_settings = bpy.props.CollectionProperty(type=Anim_property)
    bpy.utils.register_class(OT_VAT__Bake)
    bpy.utils.register_class(VAT_PT_Panel)


def unregister():
    bpy.utils.unregister_class(OT_VAT__Bake)
    bpy.utils.unregister_class(VAT_PT_Panel)

if __name__ == "__main__":
    register()


