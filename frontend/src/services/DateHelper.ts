const moscowOffset = 3 * 60 * 60 * 1000;
export function daysAgo(date: Date): number {
    const today = new Date();
    const todayMoscow = new Date(today.getTime() + moscowOffset);
    todayMoscow.setHours(0, 0, 0, 0); // Сбрасываем время на начало дня
    const inputMoscowDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    const diffTime = todayMoscow.valueOf() - inputMoscowDate.valueOf(); // Разница в миллисекундах
    const daysAgo = Math.floor(diffTime / (1000 * 60 * 60 * 24)); // Переводим в дни
    return daysAgo;
}
