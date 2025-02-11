<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import axios from 'axios';

//定义酒店接口
interface Hotel {
  hotelId: number;
  hotelName: string;
  cityName: string;
  hotelGrade: string;
  hotelLocation: string;
  hotelIntroduction: string;
}

//定义酒店房间接口
interface HotelRoom{
  hotelId: number;
  roomType: string;
  roomLeft: number;
  roomPrice: number; 
}

const route = useRoute();
const selectedNumber = ref(0);

//将时间戳进行转换
const checkInTime = computed(() => {
  return route.query.checkInTime ? new Date(parseInt(route.query.checkInTime as string)).toLocaleString() : '';
});
const checkOutTime = computed(() => {
  return route.query.checkOutTime ? new Date(parseInt(route.query.checkOutTime as string)).toLocaleString() : '';
});

const selectedHotelId = route.query.orderHotelId
const selectedRoomType = route.query.orderRoomType

const hotel = ref<Hotel[]>([]);
const hotelRooms = ref<HotelRoom[]>([]);
const checkInDate = ref<string | null>(null);
const checkOutDate = ref<string | null>(null);
const fromIndexPage =  ref<boolean>(false);

//修改显示时间格式
const formattedCheckInDate = computed(() => {
  return checkInDate.value ? new Date(checkInDate.value).toLocaleString().split(' ')[0] : '';
});
const formattedCheckOutDate = computed(() => {
  return checkOutDate.value ? new Date(checkOutDate.value).toLocaleString().split(' ')[0] : '';
});

const handleChange = () => {
  if (checkInDate.value) {
    checkInDate.value = new Date(checkInDate.value).toLocaleString(); 
  }
  if (checkOutDate.value) {
    checkOutDate.value = new Date(checkOutDate.value).toLocaleString();
  }
};

//时间判断逻辑
const disabledDate = (time: Date) => {
  const today = new Date()
  today.setHours(0, 0, 0, 0) // 仅比较日期部分，不比较时间
  return time.getTime() < today.getTime()
}

//计算入住天数
const numberOfNights = computed(() => {
  if (checkInDate.value && checkOutDate.value) {
    const tempCheckInDate = new Date(checkInDate.value);
    const tempCheckOutDate = new Date(checkOutDate.value);
    const dateDifference = tempCheckOutDate.getTime() - tempCheckInDate.getTime();
    return dateDifference > 0 ? Math.ceil(dateDifference / (1000 * 60 * 60 * 24)) : 0;
  }
  return 0;
});

//提交订单逻辑
const handleSubmit = async() => {
  if (numberOfNights.value <= 0) {
    alert("请确保入住时间和退房时间都已正确填写");
    return;
  }
  for (let i = 0; i < selectedNumber.value; i++) {
    console.log("inDate",checkInDate.value)
    console.log("outDate",checkOutDate.value)
    console.log("inTime",checkInTime.value)
    console.log("outTime",checkOutTime.value)
    try {
      const response = await axios.post(`https://123.60.14.84/api/Hotel/create`, {
        userId: Number(localStorage.getItem("id")),
        hotelId: Number(selectedHotelId),
        checkInDate: checkInDate.value,
        checkOutDate: checkOutDate.value,
        roomType: selectedRoomType,
      })
      alert(`购买成功! ${JSON.stringify(response.data.message)}`)
    } catch (error) {
      alert("购买失败，请再尝试")
      console.error("Failed to complete purchase:", error)
    }
  }
};

//判断是否禁用
const isSubmitDisabled = computed(() => {
  return selectedNumber.value === 0 || !checkInDate.value || !checkOutDate.value;
});

onMounted(() => {
  const hotelQuery = route.query.hotel as string;
  const hotelRoomsQuery = route.query.hotelRoom as string;

  // if(!checkInDate.value && !checkOutDate.value){
  //   fromIndexPage.value = true;
  // }

  hotel.value = JSON.parse(decodeURIComponent(hotelQuery));
  hotelRooms.value = JSON.parse(decodeURIComponent(hotelRoomsQuery));

  // date是提交的数据，time是原始的时间数据
  checkInDate.value = checkInTime.value;
  checkOutDate.value = checkOutTime.value;
  
});
</script>

<template>
  <div class="app-container">
    <el-card header="酒店订单">
      <div class="hotel-container" >
        <h2>订单详情</h2>
        <div v-for="hotel in hotel">
          <div class="info-container" v-for="room in hotelRooms">
            <div class="info-1" >
              <p>酒店名称: {{ hotel.hotelName }}</p>
              <p>酒店地址: {{ hotel.hotelLocation }}</p>
              <div>
                <p>房型: {{ room.roomType }}</p>
                <p>价格: ¥{{ room.roomPrice }} /间</p>
              </div>
            </div>
            <div class="info-2">
              <div class="date-selector">
                <template v-if="checkInDate && checkOutDate && fromIndexPage === true">
                  <p>入住时间：{{ formattedCheckInDate }}</p>
                  <p>退房时间：{{ formattedCheckOutDate }}</p>
                </template>
                <template v-else>
                  <p>入住时间：</p>
                  <el-date-picker 
                    v-model="checkInDate" 
                    type="datetime-local" 
                    placeholder="选择入住时间" 
                    :disabled-date="disabledDate"
                    @update = "handleChange"
                  />
                  <p>退房时间：</p>
                  <el-date-picker 
                    v-model="checkOutDate" 
                    type="datetime-local" 
                    placeholder="选择退房时间" 
                    :disabled-date="disabledDate"
                     @update = "handleChange"
                  />
                </template>
              </div>
              <div class="quantity-selector">
                <el-input-number 
                  v-model="selectedNumber" 
                  :min="0" 
                  :max="room.roomLeft" 
                />
              </div>
              <p>订单总价格: ¥{{ room.roomPrice * selectedNumber * numberOfNights }}</p>
              <button 
              :disabled="isSubmitDisabled"
              @click="handleSubmit"
              >
              提交订单
              </button>
            </div>
          </div>
        </div>
      </div>
    </el-card>
  </div>
</template>

<style scoped>
.hotel-container{
  border: 1px solid #ddd;
  padding: 15px;
  border-radius: 5px;
  max-width: 600px;
  margin: 0 auto;
}
.hotel-container h2{
  margin-left: 20px;
}
.info-container {
  margin-top: 10px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.info-1 {
  flex: 1;
  margin:0 10px
}

.info-2 {
  text-align: center;
  margin:0 10px
}


input {
  width: 100%;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

button {
  padding: 10px 20px;
  border: none;
  border-radius: 5px;
  background-color: #28a745;
  color: white;
  cursor: pointer;
}

button:hover {
  background-color: #218838;
}

button:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

.quantity-selector{
  margin: 10px 0;
}

.quantity-selector input {
  width: 80px;
  margin-right: 10px;
}

.date-selector div{
  margin: 10px 0;
}

</style>