// Разница в календарных днях в часовом поясе пользователя (так же, как выводится дата в интерфейсе).
// Date.UTC исключает ошибку на ±1 день при переходе на летнее/зимнее время.
export function daysAgo(date: Date): number {
    const today = new Date();
    const todayDay = Date.UTC(today.getFullYear(), today.getMonth(), today.getDate());
    const inputDay = Date.UTC(date.getFullYear(), date.getMonth(), date.getDate());
    return Math.round((todayDay - inputDay) / (1000 * 60 * 60 * 24));
}

export function formatDays(days: number): string {
    if (days === 0) {
        return "сегодня";
    }
    if (days === 1) {
        return "вчера";
    }

    const getDeclination = (n: number, words: [string, string, string]): string => {
        const cases = [2, 0, 1, 1, 1, 2];
        return words[
            n % 100 > 4 && n % 100 < 20
                ? 2
                : cases[Math.min(n % 10, 5)]
            ];
    };

    if (days < 7) {
        return `${days} ${getDeclination(days, ["день", "дня", "дней"])} назад`;
    }

    if (days < 60) {
        const weeks = Math.floor(days / 7);
        return `${weeks} ${getDeclination(weeks, ["неделю", "недели", "недель"])} назад`;
    }

    const months = Math.floor(days / 30);
    return `${months} ${getDeclination(months, ["месяц", "месяца", "месяцев"])} назад`;
}
