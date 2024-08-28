import { type RouteRecordRaw, createRouter } from "vue-router"
import { history, flatMultiLevelRoutes } from "./helper"
import routeSettings from "@/config/route"

const Layouts = () => import("@/layouts/index.vue")

/**
 * 常驻路由
 * 除了 redirect/403/404/login 等隐藏页面，其他页面建议设置 Name 属性
 */

export const constantRoutes: RouteRecordRaw[] = [
  
  {
    path: "/403",
    component: () => import("@/views/error-page/403.vue"),
    meta: {
      hidden: true
    }
  },
  {
    path: "/404",
    component: () => import("@/views/error-page/404.vue"),
    meta: {
      hidden: true
    },
    alias: "/:pathMatch(.*)*"
  },
  {
    path: "/login",
    component: () => import("@/views/login/index.vue"),
    meta: {
      hidden: true
    }
  },
  {
    path: "/",
    redirect: "/self-center",
  },
]

/**
 * 动态路由
 * 用来放置有权限 (Roles 属性) 的路由
 * 必须带有 Name 属性
 */
export const dynamicRoutes: RouteRecordRaw[] = [
  {
    path: "/",
    component: Layouts,
    meta:{
      roles: ["user"]
    },
    children: [
      {
        path: "dashboard",
        component: () => import("@/views/dashboard/index.vue"),
        name: "Dashboard",
        meta: {
          title: "首页",
          svgIcon: "dashboard",
          affix: true,
        }
      }
    ]
  },
  {
    path: "/",
    component: Layouts,
    meta:{
      roles: ["user","admin"]
    },
    children: [
      {
        path: "scene",
        component: () => import("@/views/scene/index.vue"),
        name: "Scene",
        meta: {
          title: "景点",
          svgIcon: "scene",
          affix: true
        }
      }
    ]
  },
  {
    path: "/scene",
    component: Layouts,
    meta: {
      hidden: true,
    },
    children: [
      {
        path: "shanghai",
        component: () => import("@/views/scene/citys/shanghai.vue"),
        name: "Shanghai",
        meta: {
          title: "上海"
        }
      },
      {
        path: "shanghai/tickets",
        component: () => import("@/views/scene/citys/city_shanghai/上海景点购票.vue"),
        name: "SceneTickets",
        meta: {
          title: "上海景点购票"
        }
      },
      {
        path: "tokyo",
        component: () => import("@/views/scene/citys/tokyo.vue"),
        name: "Tokyo",
        meta: {
          title: "东京"
        }
      },
      {
        path: "cairo",
        component: () => import("@/views/scene/citys/cairo.vue"),
        name: "Cairo",
        meta: {
          title: "开罗"
        }
      },
      {
        path: "paris",
        component: () => import("@/views/scene/citys/paris.vue"),
        name: "Paris",
        meta: {
          title: "巴黎"
        }
      },
      {
        path: "newyork",
        component: () => import("@/views/scene/citys/newyork.vue"),
        name: "Newyork",
        meta: {
          title: "纽约"
        }
      },
      {
        path: "riodejaneiro",
        component: () => import("@/views/scene/citys/riodejaneiro.vue"),
        name: "Riodejaneiro",
        meta: {
          title: "里约热内卢"
        }
      },
      {
        path: "sydney",
        component: () => import("@/views/scene/citys/sydney.vue"),
        name: "Sydney",
        meta: {
          title: "悉尼"
        }
      }
    ]
  },
  {
    path: "/",
    component: Layouts,
    meta:{
      roles: ["user","admin"]
    },
    children: [
      {
        path: "group-travel",
        component: () => import("@/views/group-travel/index.vue"),
        name: "Group-travel",
        meta: {
          title: "跟团游",
          svgIcon: "group-travel",
          affix: true
        }
      }
    ]
  },
  // csh start
  {
    path: "/group-travel",
    component: Layouts,
    meta: {
      hidden: true
    },
    children: [
    {
      path: "groups/detail",
      component: () => import(`@/views/group-travel/groups/detail.vue`), 
      name: "group-detail",
      meta: {
        title: "旅行团",
      }
  }]
  },
  //csh end
  {
    path: "/",
    component: Layouts,
    meta:{
      roles: ["guide"]
    },
    children: [
      {
        path: "guide-travel",
        component: () => import("@/views/guide-travel/index.vue"),
        name: "Guide-travel",
        meta: {
          title: "带团游",
          svgIcon: "guide-travel",
          affix: true
        }
      }
    ]
  },
  {
    path: "/",
    component: Layouts,
    children: [
      {
        path: "self-center",
        component: () => import("@/views/self-center/index.vue"),
        name: "Self-center",
        meta: {
          title: "个人中心",
          svgIcon: "self-center",
          affix: true
        }
      }
    ]
  },

  {
    path: "/",
    component: Layouts,
    meta:{
      roles: ["user","guide"]
    },
    children: [
      {
        path: "my-orders",
        component: () => import("@/views/my-orders/index.vue"),
        name: "My-orders",
        meta: {
          title: "我的订单",
          svgIcon: "my-orders",
          affix: true
        }
      }
    ]
  },
  {
    path: "/",
    component: Layouts,
    meta:{
      roles: ["admin"]
    },
    children: [
      {
        path: "orders-management",
        component: () => import("@/views/orders-management/index.vue"),
        name: "Orders-management",
        meta: {
          title: "订单管理",
          svgIcon: "Orders-management",
          affix: true
        }
      }
    ]
  },
  {
    path: "/",
    component: Layouts,
    meta:{
      roles: ["admin"]
    },
    children: [
      {
        path: "users-management",
        component: () => import("@/views/users-management/index.vue"),
        name: "Users-management",
        meta: {
          title: "用户管理",
          svgIcon: "Users-management",
          affix: true
        }
      }
    ]
  },
  {
    path: "/",
    component: Layouts,
    meta:{
      roles: ["guide"]
    },
    children: [
      {
        path: "performance",
        component: () => import("@/views/performance/index.vue"),
        name: "Performance",
        meta: {
          title: "业绩",
          svgIcon: "performance",
          affix: true
        }
      }
    ]
  },
  {
    path: "/transport",
    component: Layouts,
    redirect: "/transport/airplane",
    name: "Transport",
    meta: {
      title: "出行",
      svgIcon: "transport",
      roles: ["user","admin"]
    },
    children: [
      {
        path: "airplane",
        component: () => import("@/views/transport/airplane/index.vue"),
        name: "Airplane",
        meta: {
          title: "飞机"
        }
      },
      {
        path: "bus",
        component: () => import("@/views/transport/bus/index.vue"),
        name: "Bus",
        meta: {
          title: "大巴",
          keepAlive: true
        }
      },
      {
        path: "train",
        component: () => import("@/views/transport/train/index.vue"),
        name: "Train",
        meta: {
          title: "火车",
          keepAlive: true
        }
      }
    ]
  },
  {
    path: "/",
    component: Layouts,
    children: [
      {
        path: "hotel",
        component: () => import("@/views/hotel/index.vue"),
        name: "Hotel",
        meta: {
          title: "酒店",
          svgIcon: "hotel",
          affix: true
        }
      }
    ]
  },
  {
    path: "/hotel",
    component: Layouts,
    meta: {
      hidden: true
    },
    children: [
      {
        path: "detail",
        component: () => import("@/views/hotel/detail.vue"),
        name: "Detail",
        meta: {
          title: "酒店详情",
          hidden: true
        }
      },
      {
        path: "room",
        component: () => import("@/views/hotel/room.vue"),
        name: "Room",
        meta: {
          title: "酒店房型",
          hidden: true
        }
      },
      {
        path: "order",
        component: () => import("@/views/hotel/order.vue"),
        name: "Order",
        meta: {
          title: "酒店订单",
          hidden: true
        }
      },
    ]
  }

  // {
  //   path: "/permission",
  //   component: Layouts,
  //   redirect: "/permission/page",
  //   name: "Permission",
  //   meta: {
  //     title: "权限",
  //     svgIcon: "lock",
  //     roles: ["admin", "editor"], // 可以在根路由中设置角色
  //     alwaysShow: true // 将始终显示根菜单
  //   },
  //   children: [
  //     {
  //       path: "page",
  //       component: () => import("@/views/permission/page.vue"),
  //       name: "PagePermission",
  //       meta: {
  //         title: "页面级",
  //         roles: ["admin"] // 或者在子导航中设置角色
  //       }
  //     },
  //     {
  //       path: "directive",
  //       component: () => import("@/views/permission/directive.vue"),
  //       name: "DirectivePermission",
  //       meta: {
  //         title: "按钮级" // 如果未设置角色，则表示：该页面不需要权限，但会继承根路由的角色
  //       }
  //     }
  //   ]
  // }
]

const router = createRouter({
  history,
  routes: routeSettings.thirdLevelRouteCache ? flatMultiLevelRoutes(constantRoutes) : constantRoutes
})

/** 重置路由 */
export function resetRouter() {
  // 注意：所有动态路由路由必须带有 Name 属性，否则可能会不能完全重置干净
  try {
    router.getRoutes().forEach((route) => {
      const { name, meta } = route
      if (name && meta.roles?.length) {
        router.hasRoute(name) && router.removeRoute(name)
      }
    })
  } catch {
    // 强制刷新浏览器也行，只是交互体验不是很好
    window.location.reload()
  }
}

export default router
