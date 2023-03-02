<script lang="ts">
	import 'carbon-components-svelte/css/white.css';

	import { WalterSideNav, WalterToasts } from '$components';
	import { Modal } from 'carbon-components-svelte';
	import type { WalterModalControl } from '$types';
	import { walterModalControl } from '$store';

	let modalControl: WalterModalControl;
	walterModalControl.subscribe((value) => {
		modalControl = value;
	});
</script>

<WalterSideNav />
<div
	style="; overflow: hidden; position: absolute; top: 48px; right: 0; z-index: 99"
>
	<WalterToasts />
	<Modal
		{...modalControl}
		bind:open={modalControl.open}
		secondaryButtonText="Abbrechen"
		on:click:button--secondary={() => (modalControl.open = false)}
		on:open
		on:close
		on:submit={modalControl.submit}
	>
		<p>{modalControl.content}</p>
	</Modal>
</div>
<slot />

<style>
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

	/* :global(.bx--table-sort, .bx--table-header-label) {
		width: 100%;
		text-align: center;
	} */
</style>
