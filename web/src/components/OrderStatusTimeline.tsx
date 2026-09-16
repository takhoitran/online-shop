import type { OrderStatus } from '../types';

const HAPPY_PATH: OrderStatus[] = ['Pending', 'Approved', 'Shipping', 'Completed'];

export function OrderStatusTimeline({ status }: { status: OrderStatus }) {
  const isTerminalBranch = status === 'Cancelled' || status === 'Returned';
  const happyPathIndex = isTerminalBranch ? -1 : HAPPY_PATH.indexOf(status);

  // Cancelled can happen from Pending or Approved; Returned only after Completed — in both cases
  // show every step up to (but not including) where the order diverged from the happy path.
  const reachedBeforeBranch = status === 'Returned' ? HAPPY_PATH.length : happyPathIndex >= 0 ? happyPathIndex + 1 : 1;

  const steps = isTerminalBranch
    ? [...HAPPY_PATH.slice(0, reachedBeforeBranch), status]
    : HAPPY_PATH;

  const currentIndex = isTerminalBranch ? steps.length - 1 : happyPathIndex;

  return (
    <div className="order-timeline">
      {steps.map((step, i) => {
        const isDone = i < currentIndex;
        const isCurrent = i === currentIndex;
        const isBranch = isTerminalBranch && i === steps.length - 1;
        return (
          <div key={step} className={`order-timeline-step ${isDone ? 'done' : ''} ${isCurrent ? 'current' : ''} ${isBranch ? 'branch' : ''}`}>
            <div className="order-timeline-dot">{isDone ? '✓' : i + 1}</div>
            <span className="order-timeline-label">{step}</span>
            {i < steps.length - 1 && <div className="order-timeline-connector" />}
          </div>
        );
      })}
    </div>
  );
}
