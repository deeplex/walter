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

    function close(e: CustomEvent, index: number) {
        e.preventDefault();
        const notification = document.querySelectorAll(`.toast-notification`)[
            index
        ] as HTMLElement;

        if (notification) {
            notification.style.transition =
                'transform 1s ease, opacity 0.2s ease';
            notification.style.transform = 'translateX(200px)';
            notification.style.opacity = '0';

            setTimeout(() => removeToast(index), 200);
        } else {
            removeToast(index);
        }
    }

    let toaster: Partial<WalterToast>[] = [];

    toasts.subscribe((value) => {
        toaster = value;
    });
</script>

<div class="toast-container">
    {#each toaster as toast, i (toast)}
        <div class="toast-notification">
            <ToastNotification
                style="margin: 0px; margin-bottom: -0.25em"
                timeout={10000}
                on:close={(e) => close(e, i)}
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
        animation: progressBarAnimation 10s linear forwards; /* One-liner animation */
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
