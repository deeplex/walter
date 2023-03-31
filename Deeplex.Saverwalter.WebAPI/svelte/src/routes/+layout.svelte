<script lang="ts">
	import { Theme } from 'carbon-components-svelte';
	import 'carbon-components-svelte/css/all.css';
</script>

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

<slot />

<style>
	:global(
			a.bx--side-nav__link[aria-current='page']::before,
			a.bx--side-nav__link--current::before
		) {
		background-color: #2e7d32;
	}

	slot {
		height: 100vh - 3rem - 5px;
		width: 100vw;
		position: relative;
	}

	:global(.bx--content, .bx--accordion__content) {
		padding: 0 !important;
	}

	:global(.bx--text-input-wrapper, .bx--form-item, .bx--list-box__wrapper) {
		margin: 10px;
		flex: 1 1 auto;
		display: flex;
		flex-direction: column;
		flex-wrap: wrap;
		align-items: flex-start;
	}

	:global(
			.bx--date-picker-container,
			.flatpickr-input,
			.flatpickr-wrapper,
			.bx--date-picker.bx--date-picker--single
		) {
		width: 100%;
	}
	:global(.bx--data-table--sticky-header) {
		max-height: calc(100vh - 3rem - 5px);
	}

	:global(.bx--data-table-container) {
		min-width: 50rem;
		padding-top: 0;
	}

	:global(.bx--list-box__menu-icon, .bx--list-box__selection) {
		visibility: hidden;
	}

	:global(.bx--side-nav) {
		border-color: #666;
		border-right: 1px;
		border-style: solid;
	}

	:global(.bx--number input[type='number']) {
		min-width: 6.375rem;
		padding-right: 4em;
	}
</style>
