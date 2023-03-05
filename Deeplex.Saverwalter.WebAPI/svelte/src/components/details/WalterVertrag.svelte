<script lang="ts">
	import {
		WalterComboBox,
		WalterDatePicker,
		WalterMultiSelect,
		WalterTextInput
	} from '$WalterComponents';
	import type { WalterVertragEntry } from '$WalterTypes';
	import { Row } from 'carbon-components-svelte';

	export let a: Promise<Partial<WalterVertragEntry>> | undefined = undefined;
	export let entry: Partial<WalterVertragEntry> = {};

	// TODO
	// let vermieter = () =>
	// 	kontakte.then(async (e) => {
	// 		const besitzer = await async.then((e) => e.wohnung.besitzerId);
	// 		return e.find((f) => besitzer === f.id)?.text;
	// 	});
</script>

<Row>
	<WalterDatePicker
		bind:binding={entry.beginn}
		value={a?.then((x) => x.beginn)}
		labelText="Beginn"
	/>
	<WalterDatePicker
		bind:binding={entry.ende}
		value={a?.then((x) => x.ende)}
		labelText="Ende"
		placeholder="Offen"
	/>
</Row>
<Row>
	<WalterComboBox
		bind:binding={entry.wohnung}
		api={`/api/selection/wohnungen`}
		value={a?.then((x) => x.wohnung)}
		titleText="Wohnung"
	/>
	<!-- TODO -->
	<!-- <Column>
            <div style="margin-top:0.75rem">
                <p class="bx--label">Vermieter:</p>
                {#await vermieter()}
                    <TextInputSkeleton />
                {:then y}
                    <p style="margin-top: 0.5rem" class=".bx--text-input::placeholder">
                        {y}
                    </p>
                {/await}
            </div>
        </Column> -->
	<!-- TODO -->
	<WalterComboBox
		bind:binding={entry.ansprechpartner}
		api={`/api/selection/kontakte`}
		value={a?.then((x) => x.ansprechpartner)}
		titleText="Ansprechpartner"
	/>
</Row>
<Row>
	<WalterMultiSelect
		value={a?.then((x) => x.selectedMieter)}
		bind:binding={entry.selectedMieter}
		api="/api/selection/kontakte"
		titleText="Mieter"
	/>
</Row>
<Row>
	<WalterTextInput
		labelText="Notiz"
		bind:binding={entry.notiz}
		value={a?.then((x) => x.notiz)}
	/>
</Row>
