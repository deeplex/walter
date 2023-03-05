<script lang="ts">
	import {
		WalterDatePicker,
		WalterNumberInput,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterMieteEntry } from '$WalterTypes';

	export let a: Promise<Partial<WalterMieteEntry | undefined>> | undefined =
		undefined;
	export let entry: Partial<WalterMieteEntry> = {};
</script>

<Row>
	<WalterNumberInput
		bind:binding={entry.betrag}
		label="Betrag"
		value={a?.then((x) => x?.betrag)}
	/>
	<WalterDatePicker
		bind:binding={entry.betreffenderMonat}
		value={a?.then((x) => x?.betreffenderMonat)}
		labelText="Betreffender Monat"
	/>
	<WalterDatePicker
		bind:binding={entry.zahlungsdatum}
		value={a?.then((x) => x?.zahlungsdatum)}
		labelText="Zahlungsdatum"
	/>
</Row>
<Row>
	<WalterTextInput
		labelText="Notiz"
		bind:binding={entry.notiz}
		value={a?.then((x) => x?.notiz)}
	/>
</Row>
