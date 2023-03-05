<script lang="ts">
	import {
		WalterComboBox,
		WalterDatePicker,
		WalterMultiSelect,
		WalterTextInput
	} from '$WalterComponents';
	import type { WalterSelectionEntry, WalterVertragEntry } from '$WalterTypes';
	import { Row } from 'carbon-components-svelte';

	export let a: Partial<WalterVertragEntry> = {};
	export let wohnungen: WalterSelectionEntry[];
	export let kontakte: WalterSelectionEntry[];

	// TODO
	// let vermieter = () =>
	// 	kontakte.then(async (e) => {
	// 		const besitzer = await async.then((e) => e.wohnung.besitzerId);
	// 		return e.find((f) => besitzer === f.id)?.text;
	// 	});
</script>

<Row>
	<WalterDatePicker bind:value={a.beginn} labelText="Beginn" />
	<WalterDatePicker bind:value={a.ende} labelText="Ende" placeholder="Offen" />
</Row>
<Row>
	<WalterComboBox bind:value={a.wohnung} a={wohnungen} titleText="Wohnung" />
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
		bind:value={a.ansprechpartner}
		a={kontakte}
		titleText="Ansprechpartner"
	/>
</Row>
<Row>
	<WalterMultiSelect
		bind:value={a.selectedMieter}
		a={kontakte}
		titleText="Mieter"
	/>
</Row>
<Row>
	<WalterTextInput labelText="Notiz" bind:value={a.notiz} />
</Row>
