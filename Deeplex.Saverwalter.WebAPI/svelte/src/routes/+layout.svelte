<script lang="ts">
  import { WalterToasts } from '$WalterComponents';
  import { Theme } from 'carbon-components-svelte';
  import 'carbon-components-svelte/css/all.css';
</script>

<!-- Theming as suggested. Sidenav bg color is set in <style> -->
<Theme
  theme="g10"
  tokens={{
    'interactive-01': '#2E7D32', // darker green primary color
    'interactive-01-selected': '#2E7D32', // darker green selected color
    'active-primary': '#1D5F27',
    'link-01': '#2E7D32', // darker green link color
    focus: '#388E3C', // slightly lighter green focus color
    'hover-primary': '#388E3C' // slightly lighter green hover color
  }}
/>
<WalterToasts />
<slot />

<style>
  /* Sidenav theming doesn't seem to have a token */
  :global(
      a.bx--side-nav__link[aria-current='page']::before,
      a.bx--side-nav__link--current::before
    ) {
    background-color: #2e7d32 !important;
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

  /* Make Lists use complete screen */
  :global(.bx--data-table--sticky-header) {
    max-height: calc(100vh - 3rem - 5px) !important;
  }

  /* Adjust accordion for detail views */
  :global(.bx--content, .bx--accordion__content) {
    padding: 0 !important;
  }

  /* Adjust size of content in detail view */
  :global(.bx--text-input-wrapper, .bx--form-item, .bx--list-box__wrapper) {
    margin: 10px !important;
    flex: 1 1 auto !important;
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
