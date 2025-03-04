﻿using EntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using db_course_design.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AutoMapper;
using db_course_design.Profiles;
using db_course_design.Common;

namespace db_course_design.Services.impl
{
    public class HotelService : IHotelService
    {
        /*--上下文连接数据库--*/
        private readonly ModelContext _context;

        public IMapper _mapper { get; }

        public HotelService(ModelContext context)
        {
            _context = context;
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<HotelProfile>()).CreateMapper();
        }

        /*--返回所有酒店信息--*/
        public async Task<List<HotelResponse>> GetAllHotelsAsync()
        {
            return await _context.Hotels.Select(h => _mapper.Map<HotelResponse>(h)).ToListAsync();
        }
        /*--根据城市获取酒店信息--*/
        public async Task<List<HotelResponse>> GetHotelByCityAsync(string city)
        {
            var query = _context.Hotels.AsQueryable();
            query = query.Where(o => o.CityName.Equals(city));

            var Hotels = await query
                .Select(o => _mapper.Map<HotelResponse>(o)).ToListAsync();
            return Hotels;
        }
        /*--查询酒店房型和价格--*/
        public async Task<List<HotelTypeDetail>> GetHotelTypeDetailAsync(decimal hotelId)
        {
            var hotel = await _context.HotelRoomTypes
                .Where(o => o.HotelId == hotelId)
                .Select(o => _mapper.Map<HotelTypeDetail>(o))
                .ToListAsync();

            return hotel;
        }
        /*--检查房间在指定时间内是否可用--*/
        public async Task<bool> CheckRoomClear(HotelRoom? room, DateTime? StartDate, DateTime? EndDate)
        {
            bool isAvailable = true;

            var loadedRoom = await _context.HotelRooms
                .Include(r => r.HotelOrders)
                .ThenInclude(ho => ho.Order)
                .FirstOrDefaultAsync(r => r.HotelId == room.HotelId && r.RoomNumber == room.RoomNumber);
            if (loadedRoom == null || loadedRoom.HotelOrders == null)
            {
                Console.WriteLine("房间导航属性加载失败");
                return false;
            }

            // 遍历该房间的所有订单
            foreach (var order in loadedRoom.HotelOrders)
            {
                var status = (await _context.OrderData.FindAsync(order.OrderId)).Status;
                Console.WriteLine("订单状态为：" + status);
                if (!status.Equals("Cancelled"))
                {
                    // 检查订单日期是否与所需的日期范围重叠
                    Console.WriteLine(order.CheckOutDate + "<=" + StartDate + "或者" + order.CheckInDate + ">=" + EndDate);
                    if (!(order.CheckOutDate <= StartDate || order.CheckInDate >= EndDate))
                    {
                        isAvailable = false;
                        break;
                    }
                }
            }
            Console.WriteLine("返回值为"+ isAvailable+"！！！！！");
            return isAvailable;
        }
        /*--查询剩余房间数--*/
        public async Task<int> CountRoomLeft(string roomType, DateTime? StartDate, DateTime? EndDate)
        {
            int count = 0;
            var rooms = await _context.HotelRooms
                .Where(r => r.RoomType.Equals(roomType))
                .Include(r => r.HotelOrders)
                .ToListAsync();

            // 遍历所有属于指定房型的房间
            foreach (var room in rooms)
            {
                bool isAvailable = await CheckRoomClear(room, StartDate, EndDate);

                // 如果房间满足条件，计数+1
                if (isAvailable)
                {
                    count++;
                }
            }
            return count;
        }

        /*--返回某酒店指定房型剩余房间数和房型价格--*/
        public async Task<HotelRoomDetail?> GetHotelRoomDetailsAsync(decimal hotelId, string roomType, DateTime? StartDate, DateTime? EndDate)
        {
            var target = await _context.HotelRoomTypes.FindAsync(hotelId, roomType);

            if (target == null)
                return null;

            /*--更新符合条件的剩余房间数--*/
            int count = await CountRoomLeft(roomType, StartDate, EndDate);
            target.RoomLeft = count;
            await _context.SaveChangesAsync();

            return _mapper.Map<HotelRoomDetail>(target);
        }
        /*--返回某酒店所有房间信息--*/
        public async Task<List<HotelRoomResponse>> GetAllHotelRoomsAsync(decimal hotelId, string roomType)
        {
            var detail = (await _context.HotelRoomTypes
                .Where(t => t.HotelId == hotelId && t.RoomType == roomType)
                .Include(t => t.HotelRooms)
                .ToListAsync()).SingleOrDefault();

            if (detail == null)
                return new List<HotelRoomResponse>();
            return detail.HotelRooms.Select(r => _mapper.Map<HotelRoomResponse>(r)).ToList();
        }
        /*--创建一个酒店订单并分配房间--*/
        public async Task<string> CreateHotelOrderAsync(CreateHotelOrderRequest request)
        {
            string RoomNumber = null;

            var query = (await _context.HotelRoomTypes
                .Where(o => o.RoomType.Equals(request.RoomType) && o.HotelId == request.HotelId)
                .Include(o => o.HotelRooms)
                .ToListAsync()).SingleOrDefault();

            if (query == null)
            {
                // 输出调试信息或日志
                Console.WriteLine("No matching room type found.");
                return null;
            }
            if (query.HotelRooms == null || !query.HotelRooms.Any())
            {
                // 输出调试信息或日志
                Console.WriteLine("No rooms available for this room type.");
                return null;
            }

            // 找合适的房间
            foreach (var room in query.HotelRooms)
            {
                bool isAvailable = await CheckRoomClear(room, request.CheckInDate, request.CheckOutDate);

                if (!isAvailable)
                {
                    // 输出调试信息或日志
                    Console.WriteLine($"Room {room.RoomNumber} is not available from {request.CheckInDate} to {request.CheckOutDate}.");
                }

                if (isAvailable)
                {
                    RoomNumber = room.RoomNumber;
                    break;
                }
            }

            if (RoomNumber == null)
            {
                // 输出调试信息或日志
                Console.WriteLine("No available rooms found.");
            }

            if (RoomNumber != null)
            {
                // 添加数据到Order_Data表，保存后同步Hotel_Order
                var orderDatum = new OrderDatum
                {
                    OrderType = "hotel",
                    OrderDate = DateTime.Now,
                    UserId = request.userId,
                    Status = "Pending",
                    Price = query.RoomPrice * (request.CheckOutDate.Value - request.CheckInDate.Value).Days,
                };
                _context.OrderData.Add(orderDatum);
                await _context.SaveChangesAsync();
                var roomDetail = new HotelOrder
                {
                    OrderId = orderDatum.OrderId,
                    HotelId = request.HotelId,
                    CheckInDate = request.CheckInDate,
                    CheckOutDate = request.CheckOutDate,
                    RoomNumber = RoomNumber,
                };
                _context.HotelOrders.Add(roomDetail);
                await _context.SaveChangesAsync();
            }

            return RoomNumber;
        }

        /*--添加酒店--*/
        public async Task<HotelResponse?> AddHotelAsync(HotelRequest request)
        {
            try
            {
                var record = _mapper.Map<Hotel>(request);
                _context.Hotels.Add(record);
                await _context.SaveChangesAsync();
                return _mapper.Map<HotelResponse>(record);
            }
            catch
            {
                return null;
            }
        }
        /*--删除酒店--*/
        public async Task<bool> DeleteHotelAsync(decimal hotelId)
        {
            var target = await _context.Hotels.FindAsync(hotelId);

            if (target == null) 
                return false;

            _context.Hotels.Remove(target);
            await _context.SaveChangesAsync();
            return true;
        }
        /*--更新酒店--*/
        public async Task<HotelResponse?> UpdateHotelAsync(HotelResponse request)
        {
            try
            {
                var target = await _context.Hotels.FindAsync(request.HotelId);
                if (target == null)
                    throw new Exception();
                target.HotelName = request.HotelName;
                target.CityName = request.CityName;
                target.HotelGrade = request.HotelGrade;
                target.HotelLocation = request.HotelLocation;
                target.HotelIntroduction = request.HotelIntroduction;
                await _context.SaveChangesAsync();
                return _mapper.Map<HotelResponse>(target);
            }
            catch
            { 
                return null;
            }
        }
        /*--添加酒店房型--*/
        public async Task<HotelRoomDetail?> AddHotelRoomTypeAsync(HotelRoomTypeRequest request)
        {
            try
            {
                var record = _mapper.Map<HotelRoomType>(request);
                _context.HotelRoomTypes.Add(record);
                await _context.SaveChangesAsync();
                return _mapper.Map<HotelRoomDetail>(record);
            }
            catch
            {
                return null;
            }
        }
        /*--删除酒店房型--*/
        public async Task<bool> DeleteHotelRoomTypeAsync(decimal hotelId, string roomType)
        {
            var target = await _context.HotelRoomTypes.FindAsync(hotelId, roomType);

            if (target == null) 
                return false;

            _context.HotelRoomTypes.Remove(target);
            await _context.SaveChangesAsync(); 
            return true;
        }
        /*--更新酒店房型--*/
        public async Task<HotelRoomDetail?> UpdateHotelRoomTypeAsync(HotelRoomDetail request)
        {
            try
            {
                var target = await _context.HotelRoomTypes.FindAsync(request.HotelId, request.RoomType);
                if (target == null)
                    return null;
                target.RoomPrice = request.RoomPrice;
                target.RoomLeft = request.RoomLeft;
                await _context.SaveChangesAsync();
                return _mapper.Map<HotelRoomDetail>(target);
            }
            catch
            { 
                return null; 
            }
        }
        /*--添加酒店房间--*/
        public async Task<HotelRoomResponse?> AddHotelRoomAsync(HotelRoomRequest request)
        {
            try
            {
                var record = _mapper.Map<HotelRoom>(request);
                _context.HotelRooms.Add(record);
                await _context.SaveChangesAsync();
                return _mapper.Map<HotelRoomResponse>(record);
            }
            catch
            {
                return null;
            }
        }
        /*--删除酒店房间--*/
        public async Task<bool> DeleteHotelRoomAsync(string roomNumber, decimal hotelId)
        {
            var target = await _context.HotelRooms.FindAsync(roomNumber, hotelId);

            if (target == null) 
                return false;

            _context.HotelRooms.Remove(target);
            await _context.SaveChangesAsync();
            return true;
        }
        /*--更新酒店房间--*/
        public async Task<HotelRoomResponse?> UpdateHotelRoomAsync(HotelRoomResponse request)
        {
            try
            {
                var target = await _context.HotelRooms.FindAsync(request.RoomNumber, request.HotelId);
                if (target == null)
                    throw new Exception();
                target.RoomType = request.RoomType;
                target.RoomClear = request.RoomClear;
                await _context.SaveChangesAsync();
                return _mapper.Map<HotelRoomResponse>(target);
            }
            catch
            { 
                return null; 
            }
        }
        public async Task<IEnumerable<HotelResponse>> GetRecommendedHotels()
        {
            var recommendedHotelNames = new List<string>
            {
                "上海如家酒店（沪太路店）",
                "南京山水大酒店",
                "7天优品酒店（上海国际汽车城同济大学店）"
            };

            var recommendedHotels = await _context.Hotels
                .Where(ss => recommendedHotelNames.Contains(ss.HotelName))
                .ToListAsync();

            return _mapper.Map<IEnumerable<HotelResponse>>(recommendedHotels);
        }
    }
}
