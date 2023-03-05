<script lang="ts">
	import {
		WalterDatePicker,
		WalterNumberInput,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterMieteEntry } from '$WalterTypes';

	export let a: Partial<WalterMieteEntry> = {};
</script>

<Row>
	<WalterNumberInput bind:value={a.betrag} label="Betrag" />
	<WalterDatePicker
		bind:value={a.betreffenderMonat}
		labelText="Betreffender Monat"
	/>
	<WalterDatePicker bind:value={a.zahlungsdatum} labelText="Zahlungsdatum" />
</Row>
<Row>
	<WalterTextInput labelText="Notiz" bind:value={a.notiz} />
</Row>
