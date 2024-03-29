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
    import { beforeNavigate } from '$app/navigation';
    import { WalterToasts } from '$walter/components';
    import { walter_goto } from '$walter/services/utils';
    import { Theme } from 'carbon-components-svelte';
    import 'carbon-components-svelte/css/all.css';

    beforeNavigate((e) => {
        if (e.to && e.type !== 'goto') {
            e.cancel();
            walter_goto(e.to.url.href);
        }
    });
</script>

<!-- Theming as suggested. Sidenav bg color is set in <style> -->
<Theme
    theme="g10"
    tokens={{
        interactive: '#2E7D32',
        'interactive-01': '#2E7D32', // darker green primary color
        'interactive-01-selected': '#2E7D32', // darker green selected color
        'interactive-03': '#2E7D32',
        'interactive-04': '#2E7D32',
        'active-primary': '#1D5F27',
        'active-tertiary': '#1D5F27',
        'link-01': '#2E7D32', // darker green link color
        focus: '#388E3C', // slightly lighter green focus color
        'hover-primary': '#388E3C', // slightly lighter green hover color
        'hover-tertiary': '#388E3C' // slightly lighter green hover color
    }}
/>
<WalterToasts />
<slot />

<style>
    :global(.bx--side-nav__link) {
        padding-left: 1rem !important;
    }

    :global(.bx--side-nav__items) {
        display: flex;
        flex-direction: column;
    }

    :global(.bx--side-nav__item) {
        min-height: 32px;
    }

    :global(.bx--side-nav__divider) {
        min-height: 1px;
    }

    :global(.bx--data-table) {
        overflow: auto;
    }

    :global(.bx--modal) {
        transition: none !important;
    }

    :global(.bx--header-panel--expanded, .bx--side-nav--expanded) {
        overflow: scroll !important;
        max-height: none !important;
    }

    :global(.cds--cc--heatmap rect.heat) {
        fill: #d5d5d5;
    }

    /* Sidenav theming doesn't seem to have a token */
    :global(a) {
        color: #2e7d32;
    }
    :global(
            a.bx--side-nav__icon--small,
            a.bx--side-nav__link[aria-current='page']::before,
            a.bx--side-nav__link--current::before
        ) {
        background-color: #2e7d32 !important;
    }

    :global(.bx--file) {
        padding-top: 2em;
        display: flex;
        justify-content: center;
    }

    /* Override svelte Hamburger Menu -> Seems to be buggy with rail */
    :global(.bx--header__menu-toggle) {
        visibility: hidden !important;
        width: 8px !important;
    }

    /* Make content use the whole screen */
    slot {
        height: 100vh - 3rem - 5px !important;
        width: 100vw !important;
        position: relative !important;
    }

    /* Adjust accordion for detail views */
    :global(.bx--content, .bx--accordion__content) {
        padding: 0 !important;
    }

    :global(#homepagetabs) {
        position: fixed;
        z-index: 3000;
    }

    :global(.bx--list-box__menu-item) {
        margin: 0px !important;
    }

    :global(.bx--dropdown__wrapper) {
        margin-right: -5px !important;
    }

    :global(#usermenu > .bx--side-nav__submenu-chevron) {
        transform: scaleY(-1) !important;
    }

    /* Adjust size of content in detail view */
    :global(.bx--text-input-wrapper, .bx--form-item, .bx--list-box__wrapper) {
        margin: 10px;
        flex: 1 1 0px !important;
        display: flex !important;
        flex-direction: column !important;
        flex-wrap: wrap !important;
        align-items: flex-start !important;
    }

    /* Align datepicker to rest of elements */
    :global(
            .bx--date-picker-container,
            .flatpickr-input,
            .flatpickr-wrapper,
            .bx--date-picker.bx--date-picker--single
        ) {
        width: 100% !important;
    }

    /* Properly display lists */
    :global(.bx--data-table-container) {
        min-width: 50rem !important;
        padding-top: 0 !important;
    }

    :global(.bx--list-box__menu-icon, .bx--list-box__selection) {
        visibility: hidden !important;
    }

    /* Add border to sidenav */
    :global(.bx--side-nav) {
        border-right: 1px !important;
        border-style: solid !important;
    }

    /* Adjust size of number inputs (because +1 -1 buttons) */
    :global(.bx--number input[type='number']) {
        min-width: 6.375rem !important;
        padding-right: 4em !important;
    }
</style>
