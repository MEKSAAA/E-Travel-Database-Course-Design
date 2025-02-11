<script lang="ts" setup>
import { onMounted, ref } from "vue"
import axios from "axios"
import { ElButton, ElDialog, ElRate, ElMessage } from 'element-plus'
import { Search } from "@element-plus/icons-vue"
import { useUserStoreHook } from '@/store/modules/user'

const userStore = useUserStoreHook()
const userRole = userStore.roles
const userId = ref<string | null>(localStorage.getItem('id'))

// 接口部分
interface Guide {
  guideId: number
  guideName: string
  guideGender: string
  guideIntroduction: string
  guideSalary: number
  guidePerformanceLevel: number
  guideSeniority: string
  guidePrice: number
  imageUrl: string
}

// 数据部分
const guides = ref<Guide[]>([])
const guideId_input = ref('')
const guideName_input = ref('')
const guidePrice_input = ref<number[]>([])
const guideGrade_input = ref('')
const dialogVisible = ref(false)
const input = ref('')
const selectedGuide = ref<Guide | null>(null)
const date_input = ref('')
const showEmptyMessage = ref(false)
const busyDates = ref<{ startDate: Date; endDate: Date }[]>([])
const baseUrl = "https://123.60.14.84"

const imageMap: Record<number, string> = {
  2: "https://th.bing.com/th/id/R.d68dcaee479f08cce46b46d2f268691f?rik=1gR2azuO%2fp1%2b3g&riu=http%3a%2f%2fgss0.baidu.com%2f9fo3dSag_xI4khGko9WTAnF6hhy%2fzhidao%2fpic%2fitem%2f37d12f2eb9389b50053be23b8635e5dde6116e96.jpg&ehk=bvj7ZF98h7hwoPr%2b12EMgI%2fmXfSvZfSfzyvvz6vgjqw%3d&risl=&pid=ImgRaw&r=0",
  5: "https://th.bing.com/th/id/OIP.IXH5Twk0zpHOuxDpyhRfdwHaKl?rs=1&pid=ImgDetMain",
  //13: 'https://th.bing.com/th/id/OIP.qVCyjt_e3_xM60_2q0frjQHaKY?rs=1&pid=ImgDetMain',
  13: "https://th.bing.com/th/id/OIP.Ugi7catGMRr-tq36HUf2RQHaJ4?rs=1&pid=ImgDetMain",
  //33: 'https://th.bing.com/th/id/OIP.JoJ-AVHAUxutTyGUa3uHuAHaKk?rs=1&pid=ImgDetMain',
  33: "https://th.bing.com/th/id/OIP.TKN6MUV8TpXRSTGsXymxMwHaKX?rs=1&pid=ImgDetMain",
  //9: 'https://th.bing.com/th/id/R.36ca40fef9c4da005b251a3820d6fb4a?rik=Q6IQFoVA3Gs9Ew&riu=http%3a%2f%2ffiles.photops.com%3a81%2fattachment%2fMon_1203%2f61_207514_57205c36941dcd2.jpg%3f207&ehk=wpZxbNylcejNwiUkNkUEbVucC0%2fMWuICMBVH%2f%2bn7dNg%3d&risl=&pid=ImgRaw&r=0',
  9: "https://th.bing.com/th/id/OIP.x1wS-uaZTmUA-q3Zyz-KeAHaKi?rs=1&pid=ImgDetMain",
  10: "https://img1.baidu.com/it/u=3838128346,2925275137&fm=253&fmt=auto&app=138&f=JPEG?w=500&h=742",
  1: "https://img0.baidu.com/it/u=3193702081,1747174116&fm=253&fmt=auto&app=120&f=JPEG?w=417&h=581",
  //3: 'https://th.bing.com/th/id/OIP.TKN6MUV8TpXRSTGsXymxMwHaKX?rs=1&pid=ImgDetMain',
  3: "https://th.bing.com/th/id/R.814f0706c70345be7cc174eeef22cf97?rik=kk3NKkFwCuuGSA&riu=http%3a%2f%2fwww.86ps.com%2fUploadFiles%2fArticle%2f2018-7%2f0722%2f6.jpg&ehk=LyPuGBequu%2bBPj2lgiknapEXiLuR2RQfjKwtTPcUKFk%3d&risl=&pid=ImgRaw&r=0",
  4: "https://gss0.baidu.com/94o3dSag_xI4khGko9WTAnF6hhy/zhidao/wh%3D450%2C600/sign=5cf347cf97dda144da5c64b68787fc94/7af40ad162d9f2d30708c733a9ec8a136227ccd5.jpg",
  26: 'https://img2.baidu.com/it/u=2793308119,144286892&fm=253&fmt=auto&app=138&f=JPEG?w=500&h=700',
  34: 'https://gss0.baidu.com/-4o3dSag_xI4khGko9WTAnF6hhy/zhidao/pic/item/caef76094b36acaff26b35af78d98d1000e99cde.jpg'
}

// api部分
// 获取导游信息
const fetchGuides = () => {
  axios
    .get("https://123.60.14.84/api/Guide/all")
    .then((response) => {
      console.log("API Response:", response.data)
      const data = response.data
      if (Array.isArray(data)) {
        guides.value = data.map((guide: any) => ({
          guideId: guide.guideId,
          guideName: guide.guideName,
          guideGender: guide.guideGender,
          guideIntroduction: guide.guideIntroduction,
          guideSalary: guide.guideSalary,
          guidePerformanceLevel: Number(guide.guidePerformanceLevel),
          guideSeniority: guide.guideSeniority,
          guidePrice: guide.guidePrice,
          imageUrl: guide.profilePicture
        }))
        showEmptyMessage.value = false
      } else {
        console.error("Unexpected response format.")
        showEmptyMessage.value = true
      }
    })
    .catch((error) => {
      console.error("Error fetching guides:", error)
      showEmptyMessage.value = true
    })
}

// 根据编号、姓名筛选
const fetchById = () => {
  guideGrade_input.value = "1"
  guidePrice_input.value = []
  axios
  .get(`https://123.60.14.84/api/Guide/person`, {
      params: {
        GuideId: guideId_input.value,
        name: guideName_input.value
      }
    })
    .then((response) => {
      const data = response.data
      console.log("da:", response.data)
      if (Array.isArray(data)) {
        guides.value = data.map((guide: any) => ({
          guideId: guide.guideId,
          guideName: guide.guideName,
          guideGender: guide.guideGender,
          guideIntroduction: guide.guideIntroduction,
          guideSalary: guide.guideSalary,
          guidePerformanceLevel: Number(guide.guidePerformanceLevel),
          guideSeniority: guide.guideSeniority,
          guidePrice: guide.guidePrice,
          imageUrl: `${baseUrl}${guide.profilePicture}`
        }))
        showEmptyMessage.value = false
      } else {
        console.error("Unexpected response format.")
        showEmptyMessage.value = true
      }
    })
    .catch((error) => {
      console.error("Error fetching guides:", error)
      showEmptyMessage.value = true
    })
}

// 根据星级、价格筛选
const fetchByGrade = () => {
  guideId_input.value = ""
  guideName_input.value = ""
  axios
  .get(`https://123.60.14.84/api/Guide/ability`, {
      params: {
        minCost: guidePrice_input.value[0],
        maxCost: guidePrice_input.value[1],
        Grade: guideGrade_input.value
      }
    })
    .then((response) => {
      console.log("API Response:", response.data)
      const data = response.data
      if (Array.isArray(data)) {
        guides.value = data.map((guide: any) => ({
          guideId: guide.guideId,
          guideName: guide.guideName,
          guideGender: guide.guideGender,
          guideIntroduction: guide.guideIntroduction,
          guideSalary: guide.guideSalary,
          guidePerformanceLevel: Number(guide.guidePerformanceLevel),
          guideSeniority: guide.guideSeniority,
          guidePrice: guide.guidePrice,
          imageUrl: `${baseUrl}${guide.profilePicture}`
        }))
        showEmptyMessage.value = false
      } else {
        console.error("Unexpected response format.")
        showEmptyMessage.value = true
      }
    })
    .catch((error) => {
      console.error("Error fetching guides:", error)
      showEmptyMessage.value = true
    })
}

// 预定导游start
// 时间格式化
const formatDate = (dateString: string, time: string): string => {
  if (!dateString) {
    return "暂无"
  }
  const date = new Date(dateString)
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, "0")
  const day = String(date.getDate()).padStart(2, "0")
  return `${year}-${month}-${day}T${time}`
}
// 获取不可用时间
const disabledDateFun = (time: Record<string, any>): boolean => {
  return !busyDates.value.some((busyDate) => {
    const isDisabled = time.getTime() >= busyDate.startDate && time.getTime() <= busyDate.endDate
    console.log(isDisabled)
    return isDisabled
  })
}
// 预定弹窗
const openDialog = async (guide: Guide) => {
  selectedGuide.value = guide
  dialogVisible.value = true
  input.value = ''
  busyDates.value = []
  try {
    const response = await axios.get(`https://123.60.14.84/api/Guide/reservation/${guide.guideId}`)
    busyDates.value = response.data.map((reservation: any) => ({
      startDate: new Date(reservation.startDate).getTime(),
      endDate: new Date(reservation.endDate).getTime()
    }))
  } catch (error) {
    console.error('获取导游非空闲时间失败', error)
  }
}
// 预定
const confirm = async () => {
  console.log(userId.value,selectedGuide.value,formatDate(date_input.value[0], "00:00:00"),formatDate(date_input.value[1], "23:00:00"),input.value)
  if (selectedGuide.value && date_input.value) {
    const url = `https://123.60.14.84/api/Guide/reservation/create`
    try {
      const response = await axios.post(url, {
        userId: userId.value,
        GuideId: selectedGuide.value.guideId,
        StartDate: formatDate(date_input.value[0], "00:00:00"),
        EndDate: formatDate(date_input.value[1], "23:59:59"),
        Service: input.value
      }, {
        headers: {
          'Content-Type': 'application/json',
          'accept': '*/*'
        }
      })
      ElMessage({
            type: "success",
            message: "预定成功！"
          })
      dialogVisible.value = false
      selectedGuide.value = null
      date_input.value = ''
    } catch (error) {
      ElMessage({
            type: "info",
            message: "预定失败！"
          })
    }
  }else{
    ElMessage.info(`请选择时间`)
  }
}
// 取消预定
const cancel = () => {
  dialogVisible.value = false
  selectedGuide.value = null
  date_input.value = ''
}

onMounted(() => {
  fetchGuides()
})


</script>

<template>
<div>
    <div class="filter-container">
      <div class="first-row">
        <span class="word"> 导游编号 </span>
        <el-input
          v-model="guideId_input"
          style="width: 210px"
          placeholder="请输入导游编号"
          :prefix-icon="Search"
          @keyup.enter="fetchById"
        />

        <span class="word"> 导游姓名 </span>
        <el-input
          v-model="guideName_input"
          style="width: 210px"
          placeholder="请输入导游姓名"
          :prefix-icon="Search"
          @keyup.enter="fetchById"
        />

        <el-button type="primary" class="button" style="margin-left: 10px;" @click="fetchById" icon="Search"> 搜索 </el-button>
      </div>

      <div class="second_row">
        <span class="word"> 导游星级 </span>
        <el-input-number v-model="guideGrade_input" :min="1" :max="5" />
        <span class="word"> 导游价格 </span>
        <el-input-number v-model="guidePrice_input[0]" :min="1" :max="500000"/>
        -
        <el-input-number v-model="guidePrice_input[1]" :min="1" :max="500000"/>

        <el-button type="primary" class="button" @click="fetchByGrade" icon="Search"> 筛选 </el-button>
      </div>
    </div>

    <div class="guide-container">
        <div class="holder-container" v-if="guides.length === 0 || showEmptyMessage">
        <el-empty description="暂无导游" />
        </div>
        <div v-else v-for="guide in guides" :key="guide.guideId" class="guide-card">
            <div class="first">
                <!--<img :src="imageMap[guide.guideId]" alt="导游照片" class="photo"/>-->
                <img :src="`${baseUrl}${guide.imageUrl}`" alt="导游照片" class="photo"/>
                <div class="guide-name-gender">
                    <p class="name">{{ guide.guideName }}</p>
                    <img v-if="guide.guideGender === '男 '" src="@/views/user/guide/icon/male.svg" alt="男" class="gender-icon" />
                    <img v-else src="@/views/user/guide/icon/female.svg" alt="女" class="gender-icon" />
                </div>
            </div>

            <div class="second" >
                <p>导游编号： {{ guide.guideId }}</p>
                <div class="intro"><p>基本介绍: {{ guide.guideIntroduction }}</p></div>
                <p>价格（日）: ¥{{ guide.guidePrice }}</p>
                <el-rate
                    v-model="guide.guidePerformanceLevel"
                    disabled
                    show-score
                    text-color="#ff9900"
                    score-template="{value}星"
                />
            </div>

            <div class="third">
                <el-button type="primary" @click="openDialog(guide)">立即预定</el-button>
            </div>
        </div>
    </div>

    <el-dialog v-model="dialogVisible" title="导游预定" width="500px">
      <div>
        <span> 服务内容 </span>
        <el-input v-model="input" style="width: 210px; margin-bottom: 5px;" placeholder="请输入内容" />
      </div>
      <el-date-picker
        v-model="date_input"
        type="daterange"
        range-separator="To"
        start-placeholder="开始时间"
        end-placeholder="结束时间"
        size="default"
        :disabledDate="disabledDateFun"
      />
    <template #footer>
      <div class="dialog-footer">
        <el-button type="primary" @click="confirm">确认</el-button>
        <el-button @click="cancel">取消</el-button>
      </div>
    </template>
    </el-dialog>
</div>
</template>

<style>
.filter-container {
  margin-left: 30px;
  margin-top: 20px;
  margin-right: 40px;
  display: flex;
  gap: 5px;
  display: flex;
  flex-direction: column;
}

.word, .button{
  margin-left: 10px;
}

.guide-container {
  margin-left: 40px;
  margin-top: 20px;
  margin-right: 40px;
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
  justify-content: center;
  align-items: center;
}

.guide-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  background-color: white;
  border-radius: 10px;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  min-height: 310px;
  max-width: 100%;
  overflow: hidden;
}

.photo {
  width: 100px;
  height:140px;
  border-radius: 5%;
  margin-top: 10px;
}

.guide-name-gender {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 5px;
  width: 100%;
  margin-top: 10px;
}

.name {
  margin: 0;
  font-size: 1.2em;
  font-weight: bold;
  line-height: 1;
}
.gender-icon {
  width: 16px;
  height: 16px;
}
.second p {
  margin: 5px 0;
  line-height: 1.5;
}
.intro{
  margin-left: 5px;
}

.third {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  padding: 10px;
}

.holder-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 70vh;
  width: 100vw;
}
</style>
