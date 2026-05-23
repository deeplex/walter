<!-- Copyright (C) 2023-2024  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    import { ToastNotification } from 'carbon-components-svelte';

    import { removeToast, toasts } from '$walter/store';
    import type { WalterToast } from '$walter/types';

    const TIMEOUT = 3000;

    let toaster: Partial<WalterToast>[] = [];
    const timers = new Map<
        Partial<WalterToast>,
        ReturnType<typeof setTimeout>
    >();
    const nodes = new Map<Partial<WalterToast>, HTMLElement>();
    const remaining = new Map<Partial<WalterToast>, number>();
    const startTimes = new Map<Partial<WalterToast>, number>();

    function animateOut(node: HTMLElement, index: () => number) {
        node.style.transition = 'transform 1s ease, opacity 0.2s ease';
        node.style.transform = 'translateX(200px)';
        node.style.opacity = '0';
        setTimeout(() => removeToast(index()), 200);
    }

    function close(e: CustomEvent, toast: Partial<WalterToast>) {
        e.preventDefault();
        clearTimeout(timers.get(toast));
        timers.delete(toast);
        const node = nodes.get(toast);
        if (node) animateOut(node, () => toaster.indexOf(toast));
    }

    function toastAction(node: HTMLElement, toast: Partial<WalterToast>) {
        nodes.set(toast, node);
        const pb = node.querySelector('.progress-bar') as HTMLElement | null;

        function startAutoClose(delay = TIMEOUT) {
            clearTimeout(timers.get(toast));
            if (pb) {
                pb.style.animationName = 'none';
                void pb.offsetWidth; // force reflow to restart animation
                pb.style.removeProperty('animation-name');
                pb.style.removeProperty('animation-play-state');
            }
            startTimes.set(toast, Date.now());
            remaining.set(toast, delay);
            timers.set(
                toast,
                setTimeout(
                    () => animateOut(node, () => toaster.indexOf(toast)),
                    delay
                )
            );
        }

        startAutoClose();

        return {
            destroy() {
                clearTimeout(timers.get(toast));
                timers.delete(toast);
                nodes.delete(toast);
                remaining.delete(toast);
                startTimes.delete(toast);
            }
        };
    }

    function pauseAll() {
        for (const [toast] of timers) {
            clearTimeout(timers.get(toast));
            timers.delete(toast);
            const elapsed = Date.now() - (startTimes.get(toast) ?? Date.now());
            remaining.set(
                toast,
                Math.max(0, (remaining.get(toast) ?? TIMEOUT) - elapsed)
            );
        }
        for (const [, node] of nodes) {
            const pb = node.querySelector(
                '.progress-bar'
            ) as HTMLElement | null;
            if (pb) pb.style.animationPlayState = 'paused';
        }
    }

    function resumeAll() {
        for (const [toast, node] of nodes) {
            const rem = remaining.get(toast) ?? TIMEOUT;
            const pb = node.querySelector(
                '.progress-bar'
            ) as HTMLElement | null;
            if (pb) pb.style.animationPlayState = 'running';
            startTimes.set(toast, Date.now());
            timers.set(
                toast,
                setTimeout(
                    () => animateOut(node, () => toaster.indexOf(toast)),
                    rem
                )
            );
        }
    }

    toasts.subscribe((value) => {
        for (const [toast] of timers) {
            if (!value.includes(toast)) {
                clearTimeout(timers.get(toast));
                timers.delete(toast);
            }
        }
        toaster = value;
    });
</script>

<div
    class="toast-container"
    role="region"
    aria-label="Benachrichtigungen"
    on:mouseenter={pauseAll}
    on:mouseleave={resumeAll}
>
    {#each toaster as toast (toast)}
        <div class="toast-notification" use:toastAction={toast}>
            <ToastNotification
                style="margin: 0px; margin-bottom: -0.25em"
                timeout={0}
                on:close={(e) => close(e, toast)}
                title={toast.title}
                kind={toast.kind}
                subtitle={toast.subtitle}
                caption={new Date().toLocaleString('de-DE')}
            />
            <div
                class="progress-bar {toast.kind === 'error'
                    ? 'progress-bar-error'
                    : 'progress-bar-success'}"
            />
        </div>
    {/each}
</div>

<style>
    .toast-container {
        z-index: 9000;
        position: fixed;
        right: 0px;
        top: 48px;
    }

    .toast-notification {
        margin: 1em;
        animation-name: flyIn;
        animation-duration: 200ms;
    }

    .progress-bar {
        /* margin-bottom: -40px; */
        z-index: 9001;
        height: 0.25em;
        width: 0; /* Set initial width to 0 */
        animation: progressBarAnimation 2s linear forwards;
    }

    .progress-bar-error {
        background-color: #fa4d56 !important;
    }

    .progress-bar-success {
        background-color: #42be65 !important;
    }

    @keyframes flyIn {
        from {
            transform: translateX(200px);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }

    @keyframes progressBarAnimation {
        to {
            width: 100%; /* Change width to 'auto' */
        }
    }
</style>
