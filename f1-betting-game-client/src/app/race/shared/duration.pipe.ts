import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formats a TimeSpan (in milliseconds) to a human-readable duration string.
 * Usage: fastestLapTime | duration
 * Examples:
 *   0 -> "0:00.000"
 *   55000 -> "0:55.000"
 *   85320 -> "1:25.320"
 */
@Pipe({
  name: 'duration',
  standalone: true,
})
export class DurationPipe implements PipeTransform {
  transform(milliseconds: number | null | undefined, format: string = 'default'): string {
    if (milliseconds == null || milliseconds <= 0) {
      return '0:00.000';
    }

    const totalSeconds = Math.floor(milliseconds / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    const ms = milliseconds % 1000;

    const minStr = minutes.toString();
    const secStr = seconds.toString().padStart(2, '0');
    const msStr = ms.toString().padStart(3, '0');

    return `${minStr}:${secStr}.${msStr}`;
  }
}