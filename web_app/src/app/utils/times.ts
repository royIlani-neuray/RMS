export default class TIMES {
    static dayShiftFunc(dayShift: number): (day_id: number) => number {
        return ((day_id: number) => (day_id + dayShift) % 7);
    }

    static getUTCDiff(): number {
        const d = new Date();
        return d.getTimezoneOffset();
    }

    static convertWithMinutes(date: Date, minutesDiff: number): Date {
        return new Date(date.getTime() + minutesDiff * 60000);
    }

    static getTimeAndDayShiftOnToUTCConversion(localTimeString: string): [string, number] {
        const minutesDiff = TIMES.getUTCDiff();
        return TIMES.getTimeAndDayShiftOnConversion(localTimeString, minutesDiff);
    }

    static getTimeAndDayShiftOnFromUTCConversion(utcTimeString: string): [string, number] {
        const minutesDiff = -TIMES.getUTCDiff();
        return TIMES.getTimeAndDayShiftOnConversion(utcTimeString, minutesDiff);
    }
    
    static getTimeAndDayShiftOnConversion(timeA: string, minutesDiff: number): [string, number] {
        const parts = timeA.split(':');
        const hours = parseInt(parts[0]);
        const minutes = parseInt(parts[1]);
        const seconds = parseInt(parts[2]);
    
        const dateA = new Date();
        dateA.setHours(hours, minutes, seconds);
        const dateB = TIMES.convertWithMinutes(dateA, minutesDiff);
    
        const dayA = dateA.getDay();
        const dayB = dateB.getDay();
        const hoursB = dateB.getHours().toString().padStart(2, '0');
        const minutesB = dateB.getMinutes().toString().padStart(2, '0');
        const secondsB = dateB.getSeconds().toString().padStart(2, '0');
        const timeB = `${hoursB}:${minutesB}:${secondsB}`;
        
        let dayshift = 0;
        if (dayB > dayA) {
            dayshift = 1;
        } else if (dayB < dayA) {
            dayshift = -1;
        }
        return [timeB, dayshift];
    }
}