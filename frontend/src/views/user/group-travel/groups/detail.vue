<script lang="ts" setup>
import axios from "axios"
import { ref } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useUserStoreHook } from '../../../../store/modules/user'

const userStore = useUserStoreHook()
const userRole = userStore.roles
const userId = ref<string | null>(localStorage.getItem('id'))

// 定义路由
const route = useRoute()

// 双向绑定
const number = ref("1")

// 接收旅游团参数
const groupId = Number(route.query.groupId)
const groupName = route.query.groupName as string
const groupPrice = Number(route.query.groupPrice)
const startDate = route.query.startDate as string
const endDate = route.query.endDate as string
const departure = route.query.departure as string
const destination = route.query.destination as string
const guideName = route.query.guideName as string
const imageUrl = route.query.imageUrl as string
const tourItineraries = JSON.parse(route.query.tourItineraries as string)
const hotels = JSON.parse(route.query.hotels as string)
const goTicket = JSON.parse(route.query.goTicket as string)
const returnTicket = JSON.parse(route.query.returnTicket as string)

// 交通类型
const transportMap: Record<string, string> = {
    'plane': '飞机',
    'train': '火车高铁',
    'bus': '大巴'
}

// 时间格式
const formatDateTime = (timeString: string) => {
  const date = new Date(timeString)
  const formattedDate = date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  })
  const formattedTime = date.toLocaleTimeString('zh-CN', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  })
  return `${formattedDate} ${formattedTime}`
}

// 时长格式
const formatDuration = (timeString: string): string => {
  const [hours, minutes] = timeString.split(':').map(Number)
  if(minutes > 0){
    return `${hours}小时${minutes}分钟`
  }else{
    return `${hours}小时`
  }
}

// 加入订单购物车
const purchase = async () => {

  const url = `https://123.60.14.84/api/TourGroup/purchase/number/${number.value}`

  try {
    const response = await axios.post(url, {
      userId: userId.value,
      groupId: groupId
    }, {
      headers: {
        'Content-Type': 'application/json',
        'accept': '*/*'
      }
    })
    ElMessage({
          type: "success",
          message: "加入成功！"
        })
  } catch (error) {
    ElMessage({
          type: "info",
          message: "加入失败！"
        })
  }
}
</script>

<template>
  <div>
    <div class="group-detail">
      <div class="left-container">
        <p class="title1">{{ groupName }}</p>
        <img :src="imageUrl" alt="旅游团图片" />
        <p class="price">{{ groupPrice }} 元/人</p>
        <p>出发地：{{ departure }} - 目的地：{{ destination }}</p>
        <p>时间：{{ new Date(startDate).toLocaleDateString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit' }) }} -- {{ new Date(endDate).toLocaleDateString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit' }) }}</p>
        <p>导游: {{ guideName }}</p>
        <el-card class="ticket-box" shadow="hover">
        <div style="display: flex; align-items: center;">
          <p style="margin-right: 10px;">人数：</p>
          <el-input-number v-model="number" :min="1" :max="100" style="margin-right: 2px;" /> 
          <el-button type="primary" @click="purchase">加入订单购物车</el-button>
        </div>
        </el-card>
      </div>
      
      <div class="right-container">
        <div class="right-content-up">
          <el-scrollbar height="350px">
            <p class="title2">行程</p>
            <ul>
              <li v-for="itinerary in tourItineraries" :key="itinerary.itineraryId">
                <p>时间: {{ formatDateTime(itinerary.itineraryTime) }}</p>
                <p>时长: {{ formatDuration(itinerary.itineraryDuration) }}</p>
                <p>活动: {{ itinerary.activities }}</p>
                <hr class="custom-hr">
              </li>
            </ul>
          </el-scrollbar>
        </div>

        <div class="right-content-mid">
          <el-scrollbar height="200px">
            <p class="title2">交通</p>
            <ul>
              <li>
                <p><strong>去程：{{ transportMap[goTicket.vehicleType] }}</strong></p>
                <div v-if="goTicket.vehicleType === 'plane'">
                    <p>机票编号: {{ goTicket.ticketId }}</p>
                    <p>航班号: {{ goTicket.vehicleId }}</p>
                </div>
                <div v-else>
                  <p>车票编号: {{ goTicket.ticketId }}</p>
                  <p>车次号: {{ goTicket.vehicleId }}</p>
                </div>
                <p>座位类型: {{ goTicket.ticketType }}</p>
                <p>出发时间: {{ formatDateTime(goTicket.ticketDepartureTime) }} - 到达时间: {{ formatDateTime(goTicket.ticketArrivalTime) }}</p>
                <p>出发城市: {{ goTicket.ticketDepartureCity }} - 到达城市: {{ goTicket.ticketArrivalCity }}</p>
                <hr class="custom-hr">
              </li>
              <li>
                <p><strong>返程：{{ transportMap[returnTicket.vehicleType] }}</strong></p>
                <div v-if="returnTicket.vehicleType === 'plane'">
                    <p>机票编号: {{ returnTicket.ticketId }}</p>
                    <p>航班号: {{ returnTicket.vehicleId }}</p>
                </div>
                <div v-else>
                  <p>车票编号: {{ returnTicket.ticketId }}</p>
                  <p>车次号: {{ returnTicket.vehicleId }}</p>
                </div>
                <p>座位类型: {{ returnTicket.ticketType }}</p>
                <p>出发时间: {{ formatDateTime(returnTicket.ticketDepartureTime) }} - 到达时间: {{ formatDateTime(returnTicket.ticketArrivalTime) }}</p>
                <p>出发城市: {{ returnTicket.ticketDepartureCity }} - 到达城市: {{ returnTicket.ticketArrivalCity }}</p>
              </li>
            </ul>
          </el-scrollbar>
        </div>
        
        
        <div class="right-content-down">
          <el-scrollbar height="200px">
            <p class="title2">酒店</p>
            <ul>
              <li v-for="hotel in hotels" :key="hotel.hotelId">
                <p>酒店名称: {{ hotel.hotelName }}</p>
                <p>城市: {{ hotel.cityName }}</p>
                <p>评级: {{ hotel.hotelGrade }}</p>
                <p>位置: {{ hotel.hotelLocation }}</p>
                <p>简介: {{ hotel.hotelIntroduction }}</p>
              </li>
            </ul>
          </el-scrollbar>
        </div>
      </div>
    </div>
  </div>
  </template>
  
  
  
<style lang="css">
.group-detail {
    margin: 20px;
    display: flex;
    justify-content: space-between;
    align-items: center;
}
  
.left-container {
  flex: 0 0 40%;
  margin-right: 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center; 
  height: 700px;
}

.left-container p {
  margin: 5px;
}

.title1 {
    text-align: center;
    font-size: 2em;
    font-weight: bold;
    color: #3498db;
}

.left-container img {
    width: 90%;
    height: auto;
    border-radius: 10px;
    margin-top: 25px;
}

.price {
    text-align: center;
    font-size: 1.5em;
    font-weight: bold;
    margin-bottom: 10px;
    color: #3498db;
}
  
.right-container {
    flex: 0 0 60%;
    display: flex;
    flex-direction: column;
    justify-content: center;
    height: 90%;
}

.right-content-up {
    margin-top: 10px;
    margin-bottom: 10px;
}
  
.right-content-mid, .right-content-down {
    margin-bottom: 10px;
}
  
.el-scrollbar {
    background-color: #f8f8f8;
    border-radius: 10px;
}

.title2 {
    font-size: 1.2em;
    font-weight: bold;
    margin-bottom: 10px;
    color: #3498db;
}

.right-container p {
  margin: 20px;
}
  
ul {
    list-style: none;
    padding: 0;
}
</style>
  
  