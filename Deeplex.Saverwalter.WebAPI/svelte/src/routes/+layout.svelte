<script>
	import 'carbon-components-svelte/css/white.css';
	import { Content } from 'carbon-components-svelte';
	import { WalterSideNav } from '../components';
</script>

<WalterSideNav />

<Content>
	<slot />
</Content>

<style>
	slot {
		height: 100vh - 3rem - 5px;
		width: 100vw;
		position: relative;
	}

	:global(.bx--content) {
		padding: 0;
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

	:global(.bx--list-box__menu-icon, .bx--list-box__selection) {
		visibility: hidden;
	}

	:global(.bx--side-nav) {
		border-color: #666;
		border-right: 1px;
		border-style: solid;
	}
</style>
