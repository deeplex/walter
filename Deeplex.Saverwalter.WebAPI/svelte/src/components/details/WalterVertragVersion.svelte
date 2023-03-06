<script lang="ts">
	import {
		WalterDatePicker,
		WalterNumberInput,
		WalterTextArea
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterVertragVersionEntry } from '$WalterTypes';

	export let a: Partial<WalterVertragVersionEntry> = {};
</script>

<Row>
	<WalterDatePicker bind:value={a.beginn} labelText="Beginn" />
	<WalterNumberInput
		hideSteppers
		bind:value={a.grundmiete}
		label="Grundmiete"
	/>
	<WalterNumberInput bind:value={a.personenzahl} label="Personenzahl" />
</Row>
<Row>
	<WalterTextArea labelText="Notiz" bind:value={a.notiz} />
</Row>
