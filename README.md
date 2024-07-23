# UnityAssetsManageFramwork
基于Unity的资源管理框架

#功能目录
##Excel导出工具（进行中）
##Prefab管理工具（未开始）
##资源管理工具（未开始）
##UI管理工具（未开始）
##红点树（未开始）
##时间任务系统（未开始）
##全局/模块事件中心（未开始）

#功能介绍
##Excel导出工具
    使用ExcelDataReadeer库读取Excel数据
    增量更新Excel：依据上次记录的读取时间和Excel文件的修改时间进行增量更新，减少运行时间
    数据结构和数据分离：数据结构基于Excel字段生成，数据存储在Resources的对应JSON
